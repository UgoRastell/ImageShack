using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Backend.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;

        public ImageController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
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

            var imagePath = Path.Combine("wwwroot", "images", image.FileName);

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(imagePath));

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
    }
}
