using Microsoft.EntityFrameworkCore;
using CMCS.Data;
using CMCS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MVC
builder.Services.AddControllersWithViews();

// Add Entity Framework + SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Application Services
builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<ClaimService>();            // <-- FIX ADDED
builder.Services.AddScoped<ValidationService>();       // <-- Required by LecturerController
builder.Services.AddScoped<ClaimAutomationService>();
builder.Services.AddScoped<InvoiceGenerationService>();
builder.Services.AddScoped<ReportingService>();

// Add Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Default route (goes to Login)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
