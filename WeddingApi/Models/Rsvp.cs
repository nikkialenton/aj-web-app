namespace WeddingApi.Models;

public class Rsvp
{
    public int Id { get; set; }
    public int GuestId { get; set; }
    public Guest Guest { get; set; } = null!;

    public bool IsAttending { get; set; }
    public string Message { get; set; } = string.Empty;

    public ICollection<AdditionalGuest> AdditionalGuests { get; set; } = [];
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
