using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpendingTracker.Data;
using SpendingTracker.Models;
using SpendingTracker.Repositories;
using SpendingTracker.Repositories.Interfaces;
using SpendingTracker.Services;
using SpendingTracker.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Identity ────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ─── Authentication cookie settings ──────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
});

// ─── Repositories ─────────────────────────────────────────────────────────────
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();

// ─── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();

// ─── MVC + Razor Runtime Compilation ─────────────────────────────────────────
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

var app = builder.Build();

// ─── Middleware Pipeline ──────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ─── Auto-create DB & seed ────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    // Seed default categories if none exist
    if (!db.Categories.Any())
    {
        db.Categories.AddRange(
            new SpendingTracker.Models.Category { Id = 1, Name = "Food",          Icon = "bi-cup-hot-fill",     Color = "#FF6384", IsDefault = true, UserId = null },
            new SpendingTracker.Models.Category { Id = 2, Name = "Groceries",     Icon = "bi-basket-fill",      Color = "#36A2EB", IsDefault = true, UserId = null },
            new SpendingTracker.Models.Category { Id = 3, Name = "Utilities",     Icon = "bi-lightning-fill",   Color = "#FFCE56", IsDefault = true, UserId = null },
            new SpendingTracker.Models.Category { Id = 4, Name = "Shopping",      Icon = "bi-bag-fill",         Color = "#4BC0C0", IsDefault = true, UserId = null },
            new SpendingTracker.Models.Category { Id = 5, Name = "Transport",     Icon = "bi-car-front-fill",   Color = "#9966FF", IsDefault = true, UserId = null },
            new SpendingTracker.Models.Category { Id = 6, Name = "Entertainment", Icon = "bi-controller",       Color = "#FF9F40", IsDefault = true, UserId = null },
            new SpendingTracker.Models.Category { Id = 7, Name = "Health",        Icon = "bi-heart-pulse-fill", Color = "#FF6384", IsDefault = true, UserId = null },
            new SpendingTracker.Models.Category { Id = 8, Name = "Personal",      Icon = "bi-person-fill",      Color = "#C9CBCF", IsDefault = true, UserId = null },
            new SpendingTracker.Models.Category { Id = 9, Name = "Others",        Icon = "bi-three-dots",       Color = "#97BBCD", IsDefault = true, UserId = null }
        );
        db.SaveChanges();
    }
}

app.Run();
