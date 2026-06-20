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

        var guests = await _db.Guests
            .Include(g => g.Rsvp).ThenInclude(r => r!.AdditionalGuests)
            .OrderBy(g => g.GroupName).ThenBy(g => g.LastName).ThenBy(g => g.FirstName)
            .ToListAsync();

        return Ok(guests.Select(ToAdminDto));
    }

    // GET /api/guests/stats
    [HttpGet("stats")]
    public async Task<IActionResult> Stats([FromHeader(Name = "X-Admin-Key")] string? key)
    {
        if (!ValidKey(key)) return Unauthorized();

        var guests = await _db.Guests
            .Include(g => g.Rsvp).ThenInclude(r => r!.AdditionalGuests)
            .ToListAsync();
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
                + attending.Sum(g => g.Rsvp!.AdditionalGuests.Count())
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
            AllowedGuests = dto.AllowedGuests,
            GroupName = dto.GroupName.Trim()
        };

        _db.Guests.Add(guest);
        await _db.SaveChangesAsync();
        guest.Token = await GenerateUniqueToken();
        await _db.SaveChangesAsync();
        return Ok(ToAdminDto(guest));
    }

    // POST /api/guests/import — bulk CSV upload
    // CSV columns: FirstName, LastName, Email, AllowedGuests (number), GroupName
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

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<GuestCreateDto>().ToList();
            var added = new List<Guest>();
            var skipped = 0;
            var usedInBatch = new HashSet<string>();

            var existingNames = (await _db.Guests
                .Select(g => g.FirstName.ToLower() + "|" + g.LastName.ToLower())
                .ToListAsync()).ToHashSet();

            foreach (var dto in records)
            {
                if (string.IsNullOrWhiteSpace(dto.FirstName)) continue;
                var nameKey = (dto.FirstName?.Trim() ?? "").ToLower() + "|" + (dto.LastName?.Trim() ?? "").ToLower();
                if (existingNames.Contains(nameKey)) { skipped++; continue; }
                var guest = new Guest
                {
                    FirstName = dto.FirstName.Trim(),
                    LastName = dto.LastName.Trim(),
                    Email = dto.Email?.Trim().ToLower() ?? string.Empty,
                    AllowedGuests = dto.AllowedGuests,
                    GroupName = dto.GroupName?.Trim() ?? string.Empty
                };
                guest.Token = await GenerateUniqueToken(usedInBatch);
                usedInBatch.Add(guest.Token);
                existingNames.Add(nameKey);
                _db.Guests.Add(guest);
                added.Add(guest);
            }

            await _db.SaveChangesAsync();
            return Ok(new { imported = added.Count, skipped });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT /api/guests/{id} — update name and allowed guests
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromHeader(Name = "X-Admin-Key")] string? key,
        [FromBody] GuestUpdateDto dto)
    {
        if (!ValidKey(key)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return BadRequest(new { error = "First name and last name are required." });

        var guest = await _db.Guests.Include(g => g.Rsvp).FirstOrDefaultAsync(g => g.Id == id);
        if (guest == null) return NotFound();

        guest.FirstName = dto.FirstName.Trim();
        guest.LastName = dto.LastName.Trim();
        guest.GroupName = dto.GroupName.Trim();
        guest.AllowedGuests = dto.AllowedGuests;
        await _db.SaveChangesAsync();

        return Ok(ToAdminDto(guest));
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

        var guests = await _db.Guests
            .Include(g => g.Rsvp).ThenInclude(r => r!.AdditionalGuests)
            .OrderBy(g => g.LastName).ThenBy(g => g.FirstName).ToListAsync();

        var rows = guests.Select(g => new
        {
            g.FirstName,
            g.LastName,
            g.Email,
            g.GroupName,
            g.AllowedGuests,
            HasRsvped = g.Rsvp != null,
            IsAttending = g.Rsvp?.IsAttending,
            AdditionalGuests = g.Rsvp != null ? string.Join(", ", g.Rsvp.AdditionalGuests.Select(a => a.Name)) : null,
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
        var csv = "FirstName,LastName,Email,AllowedGuests,GroupName\n" +
                  "Maria,Santos,maria@email.com,1,Family\n" +
                  "Jose,Reyes,jose@email.com,0,Work\n";
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", "guest-import-template.csv");
    }

    private static readonly char[] _tokenChars = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

    private async Task<string> GenerateUniqueToken(HashSet<string>? reserved = null)
    {
        string token;
        do
        {
            token = new string(Enumerable.Range(0, 6)
                .Select(_ => _tokenChars[Random.Shared.Next(_tokenChars.Length)])
                .ToArray());
        }
        while (await _db.Guests.AnyAsync(g => g.Token == token) || (reserved?.Contains(token) == true));
        return token;
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
        AllowedGuests = g.AllowedGuests,
        GroupName = g.GroupName,
        HasRsvped = g.Rsvp != null,
        Rsvp = g.Rsvp == null ? null : new RsvpViewDto
        {
            IsAttending = g.Rsvp.IsAttending,
            AdditionalGuests = g.Rsvp.AdditionalGuests.Select(a => a.Name).ToList(),
            Message = g.Rsvp.Message,
            SubmittedAt = g.Rsvp.SubmittedAt
        }
    };
}
