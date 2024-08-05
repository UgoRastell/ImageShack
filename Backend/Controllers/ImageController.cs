using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Backend.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly IWebHostEnvironment _environment;


        public ImageController(DatabaseContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }

        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile image, [FromForm] Guid userId, [FromForm] bool isPublic)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image file provided.");
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var imagesFolderPath = "C:\\Users\\ugora\\source\\repos\\ImageShack\\Frontend\\wwwroot\\images\\";
            Directory.CreateDirectory(imagesFolderPath);

            var randomFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var imagePath = Path.Combine(imagesFolderPath, randomFileName);

            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var newImage = new Image
            {
                ImageId = Guid.NewGuid(),
                FileName = randomFileName,
                Url = $"/images/{randomFileName}",
                UploadDate = DateTime.UtcNow,
                IsPublic = isPublic,
                IsDeleted = false,
                UserId = userId,
                User = user
            };

            _dbContext.Images.Add(newImage);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Image uploaded successfully", ImageId = newImage.ImageId });
        }

        [HttpGet("GetPublicImages")]
        public async Task<ActionResult<IEnumerable<Image>>> GetPublicImages()
        {
            var images = await _dbContext.Images
                .Where(i => i.IsPublic && !i.IsDeleted)
                .Include(i => i.User)
                .ToListAsync();

            var imageList = images.Select(i => new
            {
                i.ImageId,
                i.FileName,
                i.Url,
                i.UploadDate,
                i.IsPublic,
                i.IsDeleted,
                i.UserId,
                i.User.Email
            });

            return Ok(imageList);
        }

        [HttpGet("GetUserImages/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserImages(Guid userId)
        {
            var images = await _dbContext.Images
                .Where(i => i.UserId == userId && !i.IsDeleted)
                .Include(i => i.User)
                .ToListAsync();

            var imageList = images.Select(i => new
            {
                i.ImageId,
                i.FileName,
                i.Url,
                i.UploadDate,
                i.IsPublic,
                i.IsDeleted,
                i.UserId,
                UserEmail = i.User.Email
            });

            return Ok(imageList);
        }

        [HttpPost("ToggleImageVisibility/{imageId}")]
        public async Task<IActionResult> ToggleImageVisibility(Guid imageId)
        {
            var image = await _dbContext.Images.FirstOrDefaultAsync(i => i.ImageId == imageId);
            if (image == null)
            {
                return NotFound("Image not found.");
            }

            image.IsPublic = !image.IsPublic;
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Image visibility toggled successfully", IsPublic = image.IsPublic });
        }

        [HttpDelete("DeleteImage/{imageId}")]
        public async Task<IActionResult> DeleteImage(Guid imageId)
        {
            var image = await _dbContext.Images.FirstOrDefaultAsync(i => i.ImageId == imageId);

            if (image == null)
            {
                return NotFound("Image not found.");
            }

            var imagePath = Path.Combine("C:\\Users\\ugora\\source\\repos\\ImageShack\\Frontend\\wwwroot\\images\\", image.FileName);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _dbContext.Images.Remove(image);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "User and associated images deleted successfully" });
        }

        [HttpDelete("DeleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var user = await _dbContext.Users
                .Include(u => u.Images)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Delete associated images from the file system
            foreach (var image in user.Images)
            {
                var imagePath = Path.Combine("C:\\Users\\ugora\\source\\repos\\ImageShack\\Frontend\\wwwroot\\images\\", image.FileName);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "User and associated images deleted successfully" });
        }

    }

}


