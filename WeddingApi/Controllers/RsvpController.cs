using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingApi.Data;
using WeddingApi.DTOs;
using WeddingApi.Models;

namespace WeddingApi.Controllers;

[ApiController]
[Route("api/rsvp")]
public class RsvpController : ControllerBase
{
    private readonly WeddingDbContext _db;

    public RsvpController(WeddingDbContext db) => _db = db;

    // GET /api/rsvp/{token} — guest loads their link
    [HttpGet("{token}")]
    public async Task<IActionResult> Lookup(string token)
    {
        var guest = await _db.Guests
            .Include(g => g.Rsvp)
            .FirstOrDefaultAsync(g => g.Token == token);

        if (guest == null)
            return NotFound(new { error = "Invitation not found." });

        return Ok(new GuestLookupDto
        {
            FirstName = guest.FirstName,
            LastName = guest.LastName,
            AllowedPlusOne = guest.AllowedPlusOne,
            HasRsvped = guest.Rsvp != null,
            ExistingRsvp = guest.Rsvp == null ? null : MapRsvpView(guest.Rsvp)
        });
    }

    // POST /api/rsvp/{token} — guest submits their RSVP
    [HttpPost("{token}")]
    public async Task<IActionResult> Submit(string token, [FromBody] RsvpCreateDto dto)
    {
        var guest = await _db.Guests
            .Include(g => g.Rsvp)
            .FirstOrDefaultAsync(g => g.Token == token);

        if (guest == null)
            return NotFound(new { error = "Invitation not found." });

        if (guest.Rsvp != null)
            return Conflict(new { error = "Already submitted.", existing = MapRsvpView(guest.Rsvp) });

        var rsvp = new Rsvp
        {
            GuestId = guest.Id,
            IsAttending = dto.IsAttending,
            PlusOneAttending = guest.AllowedPlusOne ? dto.PlusOneAttending : null,
            PlusOneName = guest.AllowedPlusOne ? dto.PlusOneName : string.Empty,
            Message = dto.Message,
            SubmittedAt = DateTime.UtcNow
        };

        _db.Rsvps.Add(rsvp);
        await _db.SaveChangesAsync();

        return Ok(MapRsvpView(rsvp));
    }

    private static RsvpViewDto MapRsvpView(Rsvp r) => new()
    {
        IsAttending = r.IsAttending,
        PlusOneAttending = r.PlusOneAttending,
        PlusOneName = r.PlusOneName,
        Message = r.Message,
        SubmittedAt = r.SubmittedAt
    };
}
