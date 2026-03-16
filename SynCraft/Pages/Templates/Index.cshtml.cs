using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Templates;

public class IndexModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public IndexModel(SynCraftDbContext db) => _db = db;

    public List<ProcessTemplate> Templates { get; set; } = [];

    public async Task OnGetAsync()
    {
        Templates = await _db.ProcessTemplates
            .Include(t => t.Steps)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var template = await _db.ProcessTemplates
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template != null)
        {
            _db.ProcessTemplates.Remove(template);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
