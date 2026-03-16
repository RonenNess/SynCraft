using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Templates;

public class EditModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public EditModel(SynCraftDbContext db) => _db = db;

    [BindProperty]
    public ProcessTemplate Template { get; set; } = new();

    [BindProperty]
    public StepTemplate NewStep { get; set; } = new();

    [BindProperty]
    public StepTemplate EditStep { get; set; } = new();

    [BindProperty]
    public MilestoneTemplate NewMilestone { get; set; } = new();

    public List<SelectListItem> PersonOptions { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var template = await _db.ProcessTemplates
            .Include(t => t.Steps)
            .ThenInclude(s => s.ResponsiblePerson)
            .Include(t => t.Milestones)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template == null)
            return NotFound();

        Template = template;
        await LoadPersonOptions();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateTemplateAsync()
    {
        var existing = await _db.ProcessTemplates.FindAsync(Template.Id);
        if (existing == null)
            return NotFound();

        existing.Name = Template.Name;
        existing.Description = Template.Description;
        existing.TargetDateComment = Template.TargetDateComment;
        await _db.SaveChangesAsync();
        return RedirectToPage("Edit", new { id = Template.Id });
    }

    public async Task<IActionResult> OnPostAddStepAsync()
    {
        var template = await _db.ProcessTemplates.FindAsync(Template.Id);
        if (template == null)
            return NotFound();

        NewStep.ProcessTemplateId = Template.Id;
        _db.StepTemplates.Add(NewStep);
        await _db.SaveChangesAsync();
        return RedirectToPage("Edit", new { id = Template.Id });
    }

    public async Task<IActionResult> OnPostDeleteStepAsync(int stepId)
    {
        var step = await _db.StepTemplates.FindAsync(stepId);
        if (step != null)
        {
            _db.StepTemplates.Remove(step);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Edit", new { id = Template.Id });
    }

    public async Task<IActionResult> OnPostUpdateStepAsync()
    {
        var step = await _db.StepTemplates.FindAsync(EditStep.Id);
        if (step == null || step.ProcessTemplateId != Template.Id)
            return NotFound();

        step.Name = EditStep.Name;
        step.Description = EditStep.Description;
        step.Category = EditStep.Category;
        step.ResponsiblePersonId = EditStep.ResponsiblePersonId;
        step.DayOffset = EditStep.DayOffset;
        step.MinDurationDays = EditStep.MinDurationDays;
        await _db.SaveChangesAsync();
        return RedirectToPage("Edit", new { id = Template.Id });
    }

    private async Task LoadPersonOptions()
    {
        var persons = await _db.Persons.OrderBy(p => p.Name).ToListAsync();
        PersonOptions = [new SelectListItem("-- Unassigned --", "")];
        PersonOptions.AddRange(persons.Select(p => new SelectListItem(p.Name, p.Id.ToString())));
    }

    public async Task<IActionResult> OnPostAddMilestoneAsync()
    {
        var template = await _db.ProcessTemplates.FindAsync(Template.Id);
        if (template == null)
            return NotFound();

        NewMilestone.ProcessTemplateId = Template.Id;
        _db.MilestoneTemplates.Add(NewMilestone);
        await _db.SaveChangesAsync();
        return RedirectToPage("Edit", new { id = Template.Id });
    }

    public async Task<IActionResult> OnPostDeleteMilestoneAsync(int milestoneId)
    {
        var milestone = await _db.MilestoneTemplates.FindAsync(milestoneId);
        if (milestone != null)
        {
            _db.MilestoneTemplates.Remove(milestone);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Edit", new { id = Template.Id });
    }
}
