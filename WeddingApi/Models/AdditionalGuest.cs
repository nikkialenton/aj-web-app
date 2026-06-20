namespace WeddingApi.Models;

public class AdditionalGuest
{
    public int Id { get; set; }
    public int RsvpId { get; set; }
    public Rsvp Rsvp { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
}
