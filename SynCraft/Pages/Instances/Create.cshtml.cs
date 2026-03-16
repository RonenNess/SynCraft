using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Instances;

public class CreateModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public CreateModel(SynCraftDbContext db) => _db = db;

    [BindProperty]
    public int SelectedTemplateId { get; set; }

    [BindProperty]
    public string InstanceName { get; set; } = string.Empty;

    [BindProperty]
    [DataType(DataType.Date)]
    public DateTime TargetDate { get; set; } = DateTime.Today.AddDays(30);

    public List<SelectListItem> TemplateOptions { get; set; } = [];

    public Dictionary<int, string> TemplateTargetDateComments { get; set; } = [];

    public async Task OnGetAsync()
    {
        await LoadTemplateOptions();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var template = await _db.ProcessTemplates
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == SelectedTemplateId);

        if (template == null)
        {
            ModelState.AddModelError("", "Selected template not found.");
            await LoadTemplateOptions();
            return Page();
        }

        if (string.IsNullOrWhiteSpace(InstanceName))
        {
            InstanceName = $"{template.Name} - {TargetDate:yyyy-MM-dd}";
        }

        var instance = new ProcessInstance
        {
            ProcessTemplateId = template.Id,
            Name = InstanceName,
            TargetDate = TargetDate,
            CreatedDate = DateTime.UtcNow,
            Status = ProcessStatus.Active
        };

        foreach (var stepTemplate in template.Steps)
        {
            instance.Steps.Add(new StepInstance
            {
                Name = stepTemplate.Name,
                Description = stepTemplate.Description,
                Category = stepTemplate.Category,
                ResponsiblePersonId = stepTemplate.ResponsiblePersonId,
                DayOffset = stepTemplate.DayOffset,
                MinDurationDays = stepTemplate.MinDurationDays,
                State = StepState.NotStarted
            });
        }

        _db.ProcessInstances.Add(instance);
        await _db.SaveChangesAsync();
        return RedirectToPage("View", new { id = instance.Id });
    }

    private async Task LoadTemplateOptions()
    {
        var templates = await _db.ProcessTemplates
            .Include(t => t.Steps)
            .OrderBy(t => t.Name)
            .ToListAsync();

        TemplateOptions = templates.Select(t =>
            new SelectListItem($"{t.Name} ({t.Steps.Count} steps)", t.Id.ToString())).ToList();

        TemplateTargetDateComments = templates
            .Where(t => !string.IsNullOrWhiteSpace(t.TargetDateComment))
            .ToDictionary(t => t.Id, t => t.TargetDateComment!);
    }
}
