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
}