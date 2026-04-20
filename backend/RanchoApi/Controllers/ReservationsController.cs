using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RanchoApi.Data;
using RanchoApi.Models;

namespace RanchoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Reservations.OrderByDescending(r => r.CreatedAt).ToListAsync());

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var reservation = await db.Reservations.FindAsync(id);
        return reservation is null ? NotFound() : Ok(reservation);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Reservation reservation)
    {
        reservation.CreatedAt = DateTime.UtcNow;
        reservation.Status = "Pending";
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var reservation = await db.Reservations.FindAsync(id);
        if (reservation is null) return NotFound();
        reservation.Status = status;
        await db.SaveChangesAsync();
        return Ok(reservation);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var reservation = await db.Reservations.FindAsync(id);
        if (reservation is null) return NotFound();
        db.Reservations.Remove(reservation);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
