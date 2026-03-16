using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Instances;

public class IndexModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public IndexModel(SynCraftDbContext db) => _db = db;

    public List<ProcessInstance> Instances { get; set; } = [];

    public async Task OnGetAsync()
    {
        Instances = await _db.ProcessInstances
            .Include(i => i.ProcessTemplate)
            .Include(i => i.Steps)
            .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var instance = await _db.ProcessInstances
            .Include(i => i.Steps)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (instance != null)
        {
            _db.ProcessInstances.Remove(instance);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
