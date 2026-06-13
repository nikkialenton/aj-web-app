using Microsoft.EntityFrameworkCore;
using WeddingApi.Models;

namespace WeddingApi.Data;

public class WeddingDbContext : DbContext
{
    public WeddingDbContext(DbContextOptions<WeddingDbContext> options) : base(options) { }

    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Rsvp> Rsvps => Set<Rsvp>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<Guest>(e =>
        {
            e.HasKey(g => g.Id);
            e.HasIndex(g => g.Token).IsUnique();
            e.Ignore(g => g.FullName);
            e.Property(g => g.FirstName).HasMaxLength(100).IsRequired();
            e.Property(g => g.LastName).HasMaxLength(100).IsRequired();
            e.Property(g => g.Email).HasMaxLength(200);
            e.Property(g => g.Token).HasMaxLength(64).IsRequired();
            e.Property(g => g.GroupName).HasMaxLength(100);
            e.HasOne(g => g.Rsvp)
             .WithOne(r => r.Guest)
             .HasForeignKey<Rsvp>(r => r.GuestId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        m.Entity<Rsvp>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.PlusOneName).HasMaxLength(200);
            e.Property(r => r.MealPreference).HasMaxLength(100);
            e.Property(r => r.PlusOneMealPreference).HasMaxLength(100);
            e.Property(r => r.DietaryRestrictions).HasMaxLength(500);
            e.Property(r => r.Message).HasMaxLength(1000);
        });
    }
}
