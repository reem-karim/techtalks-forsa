using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using techtalks_forsa.Data;
using techtalks_forsa.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// FLOW FIX: there is no email service in this MVP, so "confirm your email" only trapped people on a dead-end page.
// Accounts are now usable right after registering: Register -> signed in -> profile setup -> add skills -> Browse.
// When a real email sender exists, set this back to true.
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages()
    // FLOW FIX: signed-in users without a profile are always sent to the setup step first.
    .AddMvcOptions(o => o.Filters.Add<ProfileSetupFilter>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();