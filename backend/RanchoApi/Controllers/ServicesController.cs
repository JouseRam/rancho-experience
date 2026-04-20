using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RanchoApi.Data;
using RanchoApi.Models;

namespace RanchoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Services.Where(s => s.IsActive).OrderBy(s => s.Order).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await db.Services.FindAsync(id);
        return service is null ? NotFound() : Ok(service);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] Service service)
    {
        db.Services.Add(service);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] Service updated)
    {
        var service = await db.Services.FindAsync(id);
        if (service is null) return NotFound();
        service.Title = updated.Title;
        service.TitleEn = updated.TitleEn;
        service.Description = updated.Description;
        service.DescriptionEn = updated.DescriptionEn;
        service.Price = updated.Price;
        service.ImageUrl = updated.ImageUrl;
        service.Icon = updated.Icon;
        service.IsActive = updated.IsActive;
        service.Order = updated.Order;
        await db.SaveChangesAsync();
        return Ok(service);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await db.Services.FindAsync(id);
        if (service is null) return NotFound();
        db.Services.Remove(service);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
