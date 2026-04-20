using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RanchoApi.Data;
using RanchoApi.Models;

namespace RanchoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.ContactMessages.OrderByDescending(m => m.CreatedAt).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ContactMessage message)
    {
        message.CreatedAt = DateTime.UtcNow;
        db.ContactMessages.Add(message);
        await db.SaveChangesAsync();
        return Ok(new { success = true, message = "Mensaje recibido" });
    }

    [HttpPut("{id}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var msg = await db.ContactMessages.FindAsync(id);
        if (msg is null) return NotFound();
        msg.IsRead = true;
        await db.SaveChangesAsync();
        return Ok(msg);
    }
}
