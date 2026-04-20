using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RanchoApi.Data;
using RanchoApi.Models;

namespace RanchoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GalleryController(AppDbContext db, Cloudinary cloudinary) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category) =>
        Ok(await db.GalleryImages
            .Where(i => i.IsActive && (category == null || i.Category == category))
            .OrderBy(i => i.Order)
            .ToListAsync());

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string category, [FromForm] string altText)
    {
        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "rancho-experience"
        };

        var result = await cloudinary.UploadAsync(uploadParams);
        if (result.Error is not null)
            return BadRequest(result.Error.Message);

        var image = new GalleryImage
        {
            Url = result.SecureUrl.ToString(),
            PublicId = result.PublicId,
            Category = category,
            AltText = altText,
            Order = await db.GalleryImages.CountAsync() + 1
        };

        db.GalleryImages.Add(image);
        await db.SaveChangesAsync();
        return Ok(image);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var image = await db.GalleryImages.FindAsync(id);
        if (image is null) return NotFound();
        await cloudinary.DestroyAsync(new DeletionParams(image.PublicId));
        db.GalleryImages.Remove(image);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
