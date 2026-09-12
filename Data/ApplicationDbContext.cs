using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using techtalks_forsa.Models;

namespace techtalks_forsa.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Profile> Profiles { get; set; }

    public DbSet<MinorProfile> MinorProfiles { get; set; }

}