namespace WeddingApi.DTOs;

// Returned when guest loads their link
public class GuestLookupDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool AllowedPlusOne { get; set; }
    public bool HasRsvped { get; set; }
    public RsvpViewDto? ExistingRsvp { get; set; }
}

// Read-only view of a submitted RSVP (shown after submission)
public class RsvpViewDto
{
    public bool IsAttending { get; set; }
    public bool? PlusOneAttending { get; set; }
    public string PlusOneName { get; set; } = string.Empty;
    public string MealPreference { get; set; } = string.Empty;
    public string PlusOneMealPreference { get; set; } = string.Empty;
    public string DietaryRestrictions { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

// Submitted by the guest
public class RsvpCreateDto
{
    public bool IsAttending { get; set; }
    public bool? PlusOneAttending { get; set; }
    public string PlusOneName { get; set; } = string.Empty;
    public string MealPreference { get; set; } = string.Empty;
    public string PlusOneMealPreference { get; set; } = string.Empty;
    public string DietaryRestrictions { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

// Admin — create or import a single guest
public class GuestCreateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool AllowedPlusOne { get; set; }
    public string GroupName { get; set; } = string.Empty;
}

// Admin — full guest row with RSVP status
public class GuestAdminDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public bool AllowedPlusOne { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public bool HasRsvped { get; set; }
    public RsvpViewDto? Rsvp { get; set; }
}

// Admin stats
public class AdminStatsDto
{
    public int TotalGuests { get; set; }
    public int RsvpedCount { get; set; }
    public int PendingCount { get; set; }
    public int Attending { get; set; }
    public int Declined { get; set; }
    public int TotalAttending { get; set; } // guests + plus ones
}
