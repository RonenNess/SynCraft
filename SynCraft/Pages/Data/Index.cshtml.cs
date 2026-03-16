using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Data;

public class IndexModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public IndexModel(SynCraftDbContext db) => _db = db;

    public int PersonCount { get; set; }
    public int TemplateCount { get; set; }
    public int InstanceCount { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        PersonCount = await _db.Persons.CountAsync();
        TemplateCount = await _db.ProcessTemplates.CountAsync();
        InstanceCount = await _db.ProcessInstances.CountAsync();
    }

    public async Task<IActionResult> OnPostExportAsync()
    {
        var persons = await _db.Persons.OrderBy(p => p.Id).ToListAsync();
        var templates = await _db.ProcessTemplates
            .Include(t => t.Steps)
            .Include(t => t.Milestones)
            .OrderBy(t => t.Id)
            .ToListAsync();
        var instances = await _db.ProcessInstances
            .Include(i => i.Steps)
            .ThenInclude(s => s.Comments)
            .Include(i => i.Milestones)
            .OrderBy(i => i.Id)
            .ToListAsync();

        // Build a person ID -> name map for resolving FK references
        var personMap = persons.ToDictionary(p => p.Id, p => p.Name);

        var export = new ExportData
        {
            ExportDate = DateTime.UtcNow,
            Persons = persons.Select(p => new ExportPerson
            {
                Name = p.Name,
                Email = p.Email,
                Role = p.Role
            }).ToList(),
            Templates = templates.Select(t => new ExportTemplate
            {
                Name = t.Name,
                Description = t.Description,
                TargetDateComment = t.TargetDateComment,
                CreatedDate = t.CreatedDate,
                Steps = t.Steps.Select(s => new ExportStepTemplate
                {
                    Name = s.Name,
                    Description = s.Description,
                    Category = s.Category,
                    ResponsiblePersonName = s.ResponsiblePersonId.HasValue && personMap.ContainsKey(s.ResponsiblePersonId.Value)
                        ? personMap[s.ResponsiblePersonId.Value] : null,
                    DayOffset = s.DayOffset,
                    MinDurationDays = s.MinDurationDays
                }).ToList(),
                Milestones = t.Milestones.Select(m => new ExportMilestone
                {
                    Label = m.Label,
                    DayOffset = m.DayOffset,
                    Color = m.Color
                }).ToList()
            }).ToList(),
            Instances = instances.Select(i => new ExportInstance
            {
                Name = i.Name,
                TemplateName = templates.FirstOrDefault(t => t.Id == i.ProcessTemplateId)?.Name ?? "",
                TargetDate = i.TargetDate,
                CreatedDate = i.CreatedDate,
                Status = i.Status,
                Steps = i.Steps.Select(s => new ExportStepInstance
                {
                    Name = s.Name,
                    Description = s.Description,
                    Category = s.Category,
                    ResponsiblePersonName = s.ResponsiblePersonId.HasValue && personMap.ContainsKey(s.ResponsiblePersonId.Value)
                        ? personMap[s.ResponsiblePersonId.Value] : null,
                    DayOffset = s.DayOffset,
                    MinDurationDays = s.MinDurationDays,
                    State = s.State,
                    Comments = s.Comments.OrderBy(c => c.CreatedDate).Select(c => new ExportComment
                    {
                        Author = c.Author,
                        Text = c.Text,
                        CreatedDate = c.CreatedDate
                    }).ToList()
                }).ToList(),
                Milestones = i.Milestones.Select(m => new ExportInstanceMilestone
                {
                    Label = m.Label,
                    Date = m.Date,
                    Color = m.Color
                }).ToList()
            }).ToList()
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
        var json = JsonSerializer.Serialize(export, options);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        return File(bytes, "application/json", $"syncraft-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
    }

    public async Task<IActionResult> OnPostImportAsync(IFormFile? importFile)
    {
        if (importFile == null || importFile.Length == 0)
        {
            StatusMessage = "Error: No file selected.";
            return RedirectToPage();
        }

        ExportData? data;
        try
        {
            using var stream = importFile.OpenReadStream();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            data = await JsonSerializer.DeserializeAsync<ExportData>(stream, options);
        }
        catch (JsonException ex)
        {
            StatusMessage = $"Error: Invalid JSON file. {ex.Message}";
            return RedirectToPage();
        }

        if (data == null)
        {
            StatusMessage = "Error: Could not parse the import file.";
            return RedirectToPage();
        }

        int personsAdded = 0, templatesAdded = 0, instancesAdded = 0;

        try
        {
            // Import Persons (skip if name already exists)
            var existingPersons = await _db.Persons.ToDictionaryAsync(p => p.Name, p => p);
            foreach (var ep in data.Persons)
            {
                if (!existingPersons.ContainsKey(ep.Name))
                {
                    var person = new Person { Name = ep.Name, Email = ep.Email, Role = ep.Role };
                    _db.Persons.Add(person);
                    existingPersons[ep.Name] = person;
                    personsAdded++;
                }
            }
            await _db.SaveChangesAsync();

            // Refresh person map with IDs
            var personLookup = await _db.Persons.ToDictionaryAsync(p => p.Name, p => p.Id);

            // Import Templates (skip if name already exists)
            var existingTemplates = await _db.ProcessTemplates.Select(t => t.Name).ToListAsync();
            foreach (var et in data.Templates)
            {
                if (existingTemplates.Contains(et.Name))
                    continue;

                var template = new ProcessTemplate
                {
                    Name = et.Name,
                    Description = et.Description,
                    TargetDateComment = et.TargetDateComment,
                    CreatedDate = et.CreatedDate
                };
                _db.ProcessTemplates.Add(template);
                await _db.SaveChangesAsync();

                foreach (var es in et.Steps)
                {
                    _db.StepTemplates.Add(new StepTemplate
                    {
                        ProcessTemplateId = template.Id,
                        Name = es.Name,
                        Description = es.Description,
                        Category = es.Category,
                        ResponsiblePersonId = es.ResponsiblePersonName != null && personLookup.ContainsKey(es.ResponsiblePersonName)
                            ? personLookup[es.ResponsiblePersonName] : null,
                        DayOffset = es.DayOffset,
                        MinDurationDays = es.MinDurationDays
                    });
                }

                foreach (var em in et.Milestones)
                {
                    _db.MilestoneTemplates.Add(new MilestoneTemplate
                    {
                        ProcessTemplateId = template.Id,
                        Label = em.Label,
                        DayOffset = em.DayOffset,
                        Color = em.Color
                    });
                }

                await _db.SaveChangesAsync();
                templatesAdded++;
            }

            // Import Instances
            var templateLookup = await _db.ProcessTemplates.ToDictionaryAsync(t => t.Name, t => t.Id);
            foreach (var ei in data.Instances)
            {
                if (!templateLookup.ContainsKey(ei.TemplateName))
                    continue;

                var instance = new ProcessInstance
                {
                    ProcessTemplateId = templateLookup[ei.TemplateName],
                    Name = ei.Name,
                    TargetDate = ei.TargetDate,
                    CreatedDate = ei.CreatedDate,
                    Status = ei.Status
                };
                _db.ProcessInstances.Add(instance);
                await _db.SaveChangesAsync();

                foreach (var es in ei.Steps)
                {
                    var step = new StepInstance
                    {
                        ProcessInstanceId = instance.Id,
                        Name = es.Name,
                        Description = es.Description,
                        Category = es.Category,
                        ResponsiblePersonId = es.ResponsiblePersonName != null && personLookup.ContainsKey(es.ResponsiblePersonName)
                            ? personLookup[es.ResponsiblePersonName] : null,
                        DayOffset = es.DayOffset,
                        MinDurationDays = es.MinDurationDays,
                        State = es.State
                    };
                    _db.StepInstances.Add(step);
                    await _db.SaveChangesAsync();

                    foreach (var ec in es.Comments)
                    {
                        _db.StepComments.Add(new StepComment
                        {
                            StepInstanceId = step.Id,
                            Author = ec.Author,
                            Text = ec.Text,
                            CreatedDate = ec.CreatedDate
                        });
                    }
                }

                foreach (var em in ei.Milestones)
                {
                    _db.MilestoneInstances.Add(new MilestoneInstance
                    {
                        ProcessInstanceId = instance.Id,
                        Label = em.Label,
                        Date = em.Date,
                        Color = em.Color
                    });
                }

                await _db.SaveChangesAsync();
                instancesAdded++;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Import failed. {ex.Message}";
            return RedirectToPage();
        }

        StatusMessage = $"Import complete: {personsAdded} person(s), {templatesAdded} template(s), {instancesAdded} instance(s) added.";
        return RedirectToPage();
    }

    // ---- Export DTOs ----

    public class ExportData
    {
        public DateTime ExportDate { get; set; }
        public List<ExportPerson> Persons { get; set; } = [];
        public List<ExportTemplate> Templates { get; set; } = [];
        public List<ExportInstance> Instances { get; set; } = [];
    }

    public class ExportPerson
    {
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Role { get; set; }
    }

    public class ExportTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TargetDateComment { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<ExportStepTemplate> Steps { get; set; } = [];
        public List<ExportMilestone> Milestones { get; set; } = [];
    }

    public class ExportStepTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public StepCategory Category { get; set; }
        public string? ResponsiblePersonName { get; set; }
        public int DayOffset { get; set; }
        public int MinDurationDays { get; set; }
    }

    public class ExportMilestone
    {
        public string Label { get; set; } = string.Empty;
        public int DayOffset { get; set; }
        public MilestoneColor Color { get; set; }
    }

    public class ExportInstance
    {
        public string Name { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public DateTime TargetDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public ProcessStatus Status { get; set; }
        public List<ExportStepInstance> Steps { get; set; } = [];
        public List<ExportInstanceMilestone> Milestones { get; set; } = [];
    }

    public class ExportInstanceMilestone
    {
        public string Label { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public MilestoneColor Color { get; set; }
    }

    public class ExportStepInstance
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public StepCategory Category { get; set; }
        public string? ResponsiblePersonName { get; set; }
        public int DayOffset { get; set; }
        public int MinDurationDays { get; set; }
        public StepState State { get; set; }
        public List<ExportComment> Comments { get; set; } = [];
    }

    public class ExportComment
    {
        public string Author { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
