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

            var imagesFolderPath = "C:/Users/ugora/source/repos/Backend/Frontend/wwwroot/images/";
            var imagePath = Path.Combine(imagesFolderPath, image.FileName);

            // Ensure the directory exists
            Directory.CreateDirectory(imagesFolderPath);

            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var newImage = new Image
            {
                ImageId = Guid.NewGuid(),
                FileName = image.FileName,
                Url = $"/images/{image.FileName}",
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
    }
}
