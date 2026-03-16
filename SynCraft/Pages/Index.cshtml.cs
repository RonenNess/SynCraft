using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages;

public class IndexModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public IndexModel(SynCraftDbContext db) => _db = db;

    public List<ProcessGroupViewModel> ProcessGroups { get; set; } = [];
    public int TotalActive { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalPersons { get; set; }
    public int TotalTemplates { get; set; }

    public async Task OnGetAsync()
    {
        var instances = await _db.ProcessInstances
            .Include(i => i.ProcessTemplate)
            .Include(i => i.Steps)
            .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();

        TotalActive = instances.Count(i => i.Status == ProcessStatus.Active);
        TotalCompleted = instances.Count(i => i.Status == ProcessStatus.Completed);
        TotalPersons = await _db.Persons.CountAsync();
        TotalTemplates = await _db.ProcessTemplates.CountAsync();

        ProcessGroups = instances
            .GroupBy(i => i.ProcessTemplate?.Name ?? "Unknown")
            .OrderBy(g => g.Key)
            .Select(g => new ProcessGroupViewModel
            {
                TemplateName = g.Key,
                Instances = g.ToList()
            })
            .ToList();
    }

    public class ProcessGroupViewModel
    {
        public string TemplateName { get; set; } = string.Empty;
        public List<ProcessInstance> Instances { get; set; } = [];
    }
}
