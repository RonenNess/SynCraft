using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Templates;

public class DeleteModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public DeleteModel(SynCraftDbContext db) => _db = db;

    public ProcessTemplate Template { get; set; } = new();
    public int InstanceCount { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var template = await _db.ProcessTemplates
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template == null)
            return NotFound();

        Template = template;
        InstanceCount = await _db.ProcessInstances.CountAsync(i => i.ProcessTemplateId == id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var template = await _db.ProcessTemplates
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template != null)
        {
            _db.ProcessTemplates.Remove(template);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}
