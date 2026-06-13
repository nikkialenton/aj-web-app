namespace WeddingApi.Models;

public class Rsvp
{
    public int Id { get; set; }
    public int GuestId { get; set; }
    public Guest Guest { get; set; } = null!;

    public bool IsAttending { get; set; }
    public bool? PlusOneAttending { get; set; }   // null = no +1 allowed
    public string PlusOneName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
