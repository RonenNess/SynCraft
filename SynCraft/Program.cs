using Microsoft.EntityFrameworkCore;
using SynCraft.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddDbContext<SynCraftDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=syncraft.db"));

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SynCraftDbContext>();
    db.Database.EnsureCreated();

    // Add columns/tables that may not exist yet in an older database
    try { db.Database.ExecuteSqlRaw("ALTER TABLE ProcessTemplates ADD COLUMN TargetDateComment TEXT NULL"); }
    catch { /* Column already exists */ }

    try { db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS MilestoneTemplates (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        ProcessTemplateId INTEGER NOT NULL,
        Label TEXT NOT NULL,
        DayOffset INTEGER NOT NULL,
        Color INTEGER NOT NULL DEFAULT 1,
        FOREIGN KEY (ProcessTemplateId) REFERENCES ProcessTemplates(Id) ON DELETE CASCADE
    )"); }
    catch { /* Table already exists */ }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
