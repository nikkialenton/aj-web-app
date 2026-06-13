using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WeddingApi.Data;
using WeddingApi.DTOs;
using WeddingApi.Models;

namespace WeddingApi.Controllers;

[ApiController]
[Route("api/guests")]
public class GuestController : ControllerBase
{
    private readonly WeddingDbContext _db;
    private readonly IConfiguration _config;

    public GuestController(WeddingDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // GET /api/guests — all guests with RSVP status
    [HttpGet]
    public async Task<IActionResult> GetAll([FromHeader(Name = "X-Admin-Key")] string? key)
    {
        if (!ValidKey(key)) return Unauthorized();

        var guests = await _db.Guests.Include(g => g.Rsvp)
            .OrderBy(g => g.GroupName).ThenBy(g => g.LastName).ThenBy(g => g.FirstName)
            .ToListAsync();

        return Ok(guests.Select(ToAdminDto));
    }

    // GET /api/guests/stats
    [HttpGet("stats")]
    public async Task<IActionResult> Stats([FromHeader(Name = "X-Admin-Key")] string? key)
    {
        if (!ValidKey(key)) return Unauthorized();

        var guests = await _db.Guests.Include(g => g.Rsvp).ToListAsync();
        var rsvped = guests.Where(g => g.Rsvp != null).ToList();
        var attending = rsvped.Where(g => g.Rsvp!.IsAttending).ToList();

        return Ok(new AdminStatsDto
        {
            TotalGuests = guests.Count,
            RsvpedCount = rsvped.Count,
            PendingCount = guests.Count - rsvped.Count,
            Attending = attending.Count,
            Declined = rsvped.Count - attending.Count,
            TotalAttending = attending.Count
                + attending.Count(g => g.Rsvp!.PlusOneAttending == true)
        });
    }

    // POST /api/guests — add single guest
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "X-Admin-Key")] string? key,
        [FromBody] GuestCreateDto dto)
    {
        if (!ValidKey(key)) return Unauthorized();

        var guest = new Guest
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            AllowedPlusOne = dto.AllowedPlusOne,
            GroupName = dto.GroupName.Trim()
        };

        _db.Guests.Add(guest);
        await _db.SaveChangesAsync();
        guest.Token = await GenerateUniqueToken(guest.FullName);
        await _db.SaveChangesAsync();
        return Ok(ToAdminDto(guest));
    }

    // POST /api/guests/import — bulk CSV upload
    // CSV columns: FullName, Email, AllowedPlusOne (true/false), GroupName
    [HttpPost("import")]
    public async Task<IActionResult> Import(
        [FromHeader(Name = "X-Admin-Key")] string? key,
        IFormFile file)
    {
        if (!ValidKey(key)) return Unauthorized();
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<GuestCreateDto>().ToList();
        var added = new List<Guest>();

        foreach (var dto in records)
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName)) continue;
            var guest = new Guest
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email?.Trim().ToLower() ?? string.Empty,
                AllowedPlusOne = dto.AllowedPlusOne,
                GroupName = dto.GroupName?.Trim() ?? string.Empty
            };
            _db.Guests.Add(guest);
            added.Add(guest);
        }

        await _db.SaveChangesAsync();
        var usedInBatch = new HashSet<string>();
        foreach (var g in added)
        {
            g.Token = await GenerateUniqueToken(g.FullName, usedInBatch);
            usedInBatch.Add(g.Token);
        }
        await _db.SaveChangesAsync();
        return Ok(new { imported = added.Count });
    }

    // DELETE /api/guests/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromHeader(Name = "X-Admin-Key")] string? key)
    {
        if (!ValidKey(key)) return Unauthorized();
        var guest = await _db.Guests.FindAsync(id);
        if (guest == null) return NotFound();
        _db.Guests.Remove(guest);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/guests/export — download all as CSV
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromHeader(Name = "X-Admin-Key")] string? key)
    {
        if (!ValidKey(key)) return Unauthorized();

        var guests = await _db.Guests.Include(g => g.Rsvp)
            .OrderBy(g => g.LastName).ThenBy(g => g.FirstName).ToListAsync();

        var rows = guests.Select(g => new
        {
            g.FirstName,
            g.LastName,
            g.Email,
            g.GroupName,
            g.AllowedPlusOne,
            HasRsvped = g.Rsvp != null,
            IsAttending = g.Rsvp?.IsAttending,
            PlusOneAttending = g.Rsvp?.PlusOneAttending,
            PlusOneName = g.Rsvp?.PlusOneName,
            Message = g.Rsvp?.Message,
            SubmittedAt = g.Rsvp?.SubmittedAt
        });

        var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            csv.WriteRecords(rows);

        stream.Position = 0;
        return File(stream, "text/csv", $"guests-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    // GET /api/guests/template — download blank CSV template
    [HttpGet("template")]
    public IActionResult Template([FromHeader(Name = "X-Admin-Key")] string? key)
    {
        if (!ValidKey(key)) return Unauthorized();
        var csv = "FirstName,LastName,Email,AllowedPlusOne,GroupName\n" +
                  "Maria,Santos,maria@email.com,true,Family\n" +
                  "Jose,Reyes,jose@email.com,false,Work\n";
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", "guest-import-template.csv");
    }

    private async Task<string> GenerateUniqueToken(string fullName, HashSet<string>? reserved = null)
    {
        var slug = new string(fullName.ToLowerInvariant()
            .Replace(' ', '-')
            .Where(c => char.IsAsciiLetterOrDigit(c) || c == '-')
            .ToArray());

        if (!await _db.Guests.AnyAsync(g => g.Token == slug) && (reserved == null || !reserved.Contains(slug)))
            return slug;

        var suffix = 2;
        while (await _db.Guests.AnyAsync(g => g.Token == $"{slug}-{suffix}") || (reserved?.Contains($"{slug}-{suffix}") == true))
            suffix++;

        return $"{slug}-{suffix}";
    }

    private bool ValidKey(string? key) =>
        !string.IsNullOrWhiteSpace(key) && key == _config["AdminKey"];

    private static GuestAdminDto ToAdminDto(Guest g) => new()
    {
        Id = g.Id,
        FirstName = g.FirstName,
        LastName = g.LastName,
        Email = g.Email,
        Token = g.Token,
        AllowedPlusOne = g.AllowedPlusOne,
        GroupName = g.GroupName,
        HasRsvped = g.Rsvp != null,
        Rsvp = g.Rsvp == null ? null : new RsvpViewDto
        {
            IsAttending = g.Rsvp.IsAttending,
            PlusOneAttending = g.Rsvp.PlusOneAttending,
            PlusOneName = g.Rsvp.PlusOneName,
            Message = g.Rsvp.Message,
            SubmittedAt = g.Rsvp.SubmittedAt
        }
    };
}
