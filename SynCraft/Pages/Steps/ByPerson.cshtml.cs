using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Steps;

public class ByPersonModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public ByPersonModel(SynCraftDbContext db) => _db = db;

    public List<PersonStepsGroup> Groups { get; set; } = [];

    public async Task OnGetAsync()
    {
        var openSteps = await _db.StepInstances
            .Include(s => s.ResponsiblePerson)
            .Include(s => s.ProcessInstance)
            .Where(s => s.State != StepState.Done
                     && s.State != StepState.Cancelled
                     && s.ProcessInstance!.Status == ProcessStatus.Active)
            .OrderBy(s => s.ResponsiblePerson != null ? s.ResponsiblePerson.Name : "")
            .ToListAsync();

        Groups = openSteps
            .GroupBy(s => new
            {
                Id = s.ResponsiblePersonId ?? 0,
                Name = s.ResponsiblePerson?.Name ?? "Unassigned"
            })
            .OrderBy(g => g.Key.Name)
            .Select(g => new PersonStepsGroup
            {
                PersonName = g.Key.Name,
                Steps = g.OrderBy(s => s.Deadline).ToList()
            })
            .ToList();
    }

    public class PersonStepsGroup
    {
        public string PersonName { get; set; } = string.Empty;
        public List<StepInstance> Steps { get; set; } = [];
    }
}
