using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using techtalks_forsa.Models;

namespace techtalks_forsa.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext(options)
{
    public DbSet<Profile> Profiles { get; set; }

    public DbSet<MinorProfile> MinorProfiles { get; set; }

    public DbSet<Listing> Listings { get; set; }

    public DbSet<SwapRequest> SwapRequests { get; set; }

    public DbSet<Message> Messages { get; set; }

    public DbSet<Session> Sessions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // One profile per user (A5). Also makes UserId indexable (nvarchar(450)).
        builder.Entity<Profile>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        builder.Entity<Listing>()
            .HasIndex(l => l.OwnerUserId);

        // One OPEN (Pending or Accepted) request per requester + listing (A2, B1).
        // Status values: Pending = 0, Accepted = 1, Declined = 2.
        builder.Entity<SwapRequest>()
            .HasIndex(r => new { r.RequesterUserId, r.TargetListingId })
            .IsUnique()
            .HasFilter("[Status] IN (0, 1)");

        builder.Entity<SwapRequest>()
            .HasIndex(r => r.RequesterUserId);

        builder.Entity<Message>()
            .HasIndex(m => new { m.SenderId, m.ReceiverId });

        // One session per swap request (C1): decide rules BEFORE building the Sessions pages.
        builder.Entity<Session>()
            .HasIndex(s => s.SwapRequestId)
            .IsUnique();
    }
}
