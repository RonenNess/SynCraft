using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Instances;

public class ViewModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public ViewModel(SynCraftDbContext db) => _db = db;

    public ProcessInstance Instance { get; set; } = new();

    /// <summary>
    /// Steps grouped by responsible person, ordered by deadline.
    /// </summary>
    public List<PersonStepGroup> StepGroups { get; set; } = [];

    /// <summary>
    /// Lane index assigned to each step (keyed by StepInstance.Id).
    /// </summary>
    public Dictionary<int, int> StepLanes { get; set; } = [];

    /// <summary>
    /// Number of lanes required for each person group (keyed by PersonName).
    /// </summary>
    public Dictionary<string, int> GroupLaneCount { get; set; } = [];

    /// <summary>
    /// Height in pixels of a single lane in the timeline.
    /// </summary>
    public const int LaneHeight = 50;

    /// <summary>
    /// The earliest date on the timeline (earliest step start or today, whichever is earlier).
    /// </summary>
    public DateTime TimelineStart { get; set; }

    /// <summary>
    /// The latest date on the timeline.
    /// </summary>
    public DateTime TimelineEnd { get; set; }

    /// <summary>
    /// Total days span of the timeline.
    /// </summary>
    public int TimelineDays { get; set; }

    /// <summary>
    /// Milestones from the process template, resolved to actual dates.
    /// </summary>
    public List<ResolvedMilestone> Milestones { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var instance = await _db.ProcessInstances
            .Include(i => i.ProcessTemplate)
            .Include(i => i.Steps)
            .ThenInclude(s => s.ResponsiblePerson)
            .Include(i => i.Steps)
            .ThenInclude(s => s.Comments)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (instance == null)
            return NotFound();

        Instance = instance;

        // Load milestones from template
        var milestoneTemplates = await _db.MilestoneTemplates
            .Where(m => m.ProcessTemplateId == instance.ProcessTemplateId)
            .OrderBy(m => m.DayOffset)
            .ToListAsync();

        Milestones = milestoneTemplates.Select(m => new ResolvedMilestone
        {
            Label = m.Label,
            Date = instance.TargetDate.AddDays(m.DayOffset),
            Color = m.Color
        }).ToList();

        // Load instance-specific milestones
        var instanceMilestones = await _db.MilestoneInstances
            .Where(m => m.ProcessInstanceId == instance.Id)
            .OrderBy(m => m.Date)
            .ToListAsync();

        Milestones.AddRange(instanceMilestones.Select(m => new ResolvedMilestone
        {
            Label = m.Label,
            Date = m.Date,
            Color = m.Color,
            InstanceMilestoneId = m.Id
        }));

        Milestones = Milestones.OrderBy(m => m.Date).ToList();

        BuildTimeline();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStepAsync(int id, int stepId, StepState newState)
    {
        var step = await _db.StepInstances.FindAsync(stepId);
        if (step == null || step.ProcessInstanceId != id)
            return NotFound();

        step.State = newState;
        await _db.SaveChangesAsync();

        return RedirectToPage("View", new { id });
    }

    public async Task<IActionResult> OnPostCompleteProcessAsync(int id)
    {
        var instance = await _db.ProcessInstances
            .Include(i => i.Steps)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (instance == null)
            return NotFound();

        if (instance.CanComplete)
        {
            instance.Status = ProcessStatus.Completed;
            await _db.SaveChangesAsync();
        }

        return RedirectToPage("View", new { id });
    }

    public async Task<IActionResult> OnPostAddCommentAsync(int id, int stepId, string author, string text)
    {
        var step = await _db.StepInstances.FindAsync(stepId);
        if (step == null || step.ProcessInstanceId != id)
            return NotFound();

        if (string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(text))
            return RedirectToPage("View", new { id });

        _db.StepComments.Add(new StepComment
        {
            StepInstanceId = stepId,
            Author = author.Trim(),
            Text = text.Trim(),
            CreatedDate = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return RedirectToPage("View", new { id, fragment = $"step-card-{stepId}" });
    }

    public async Task<IActionResult> OnPostPushDeadlineAsync(int id, int pushDays)
    {
        if (pushDays <= 0)
            return RedirectToPage("View", new { id });

        var instance = await _db.ProcessInstances
            .Include(i => i.Steps)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (instance == null)
            return NotFound();

        if (instance.Status != ProcessStatus.Active)
            return RedirectToPage("View", new { id });

        // Create an instance milestone marking the original target date
        _db.MilestoneInstances.Add(new MilestoneInstance
        {
            ProcessInstanceId = instance.Id,
            Label = "Original Target",
            Date = instance.TargetDate,
            Color = MilestoneColor.Red
        });

        // Adjust DayOffset of Done/Cancelled steps so their deadlines stay the same
        // after the target date is pushed forward
        foreach (var step in instance.Steps)
        {
            if (step.State == StepState.Done || step.State == StepState.Cancelled)
                step.DayOffset -= pushDays;
        }

        // Push the target date
        instance.TargetDate = instance.TargetDate.AddDays(pushDays);

        await _db.SaveChangesAsync();

        return RedirectToPage("View", new { id });
    }

    public async Task<IActionResult> OnPostDeleteProcessAsync(int id)
    {
        var instance = await _db.ProcessInstances
            .Include(i => i.Steps)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (instance != null)
        {
            _db.ProcessInstances.Remove(instance);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("/Instances/Index");
    }

    public async Task<IActionResult> OnPostAddMilestoneAsync(int id, string label, DateTime date, MilestoneColor color)
    {
        var instance = await _db.ProcessInstances.FindAsync(id);
        if (instance == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(label))
            return RedirectToPage("View", new { id });

        _db.MilestoneInstances.Add(new MilestoneInstance
        {
            ProcessInstanceId = id,
            Label = label.Trim(),
            Date = date,
            Color = color
        });
        await _db.SaveChangesAsync();

        return RedirectToPage("View", new { id });
    }

    public async Task<IActionResult> OnPostDeleteMilestoneAsync(int id, int milestoneId)
    {
        var milestone = await _db.MilestoneInstances.FindAsync(milestoneId);
        if (milestone != null && milestone.ProcessInstanceId == id)
        {
            _db.MilestoneInstances.Remove(milestone);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("View", new { id });
    }

    private void BuildTimeline()
    {
        if (Instance.Steps.Count == 0)
        {
            TimelineStart = DateTime.Today;
            TimelineEnd = Instance.TargetDate;
            TimelineDays = Math.Max(1, (TimelineEnd - TimelineStart).Days);
            return;
        }

        var allDates = new List<DateTime>();
        foreach (var step in Instance.Steps)
        {
            allDates.Add(step.Deadline);
            if (step.MinDurationDays > 0)
                allDates.Add(step.MustStartBy);
        }
        allDates.Add(Instance.TargetDate);
        allDates.Add(DateTime.Today);
        foreach (var ms in Milestones)
            allDates.Add(ms.Date);

        TimelineStart = allDates.Min().AddDays(-15);
        TimelineEnd = allDates.Max().AddDays(15);
        TimelineDays = Math.Max(1, (TimelineEnd - TimelineStart).Days);

        StepGroups = Instance.Steps
            .GroupBy(s => new { Id = s.ResponsiblePersonId ?? 0, Name = s.ResponsiblePerson?.Name ?? "Unassigned" })
            .OrderBy(g => g.Key.Name)
            .Select(g => new PersonStepGroup
            {
                PersonName = g.Key.Name,
                Steps = g.OrderBy(s => s.Deadline).ToList()
            })
            .ToList();

        AssignLanes();
    }

    private void AssignLanes()
    {
        const double overlapThreshold = 8.0;

        foreach (var group in StepGroups)
        {
            var lanes = new List<List<(double start, double end)>>();

            foreach (var step in group.Steps)
            {
                var deadlinePos = GetPosition(step.Deadline);
                var startPos = step.MinDurationDays > 0 ? GetPosition(step.MustStartBy) : deadlinePos;
                var rangeStart = startPos - overlapThreshold;
                var rangeEnd = deadlinePos + overlapThreshold;

                int assignedLane = -1;
                for (int i = 0; i < lanes.Count; i++)
                {
                    if (!lanes[i].Any(r => rangeStart < r.end && rangeEnd > r.start))
                    {
                        assignedLane = i;
                        break;
                    }
                }

                if (assignedLane == -1)
                {
                    assignedLane = lanes.Count;
                    lanes.Add([]);
                }

                lanes[assignedLane].Add((rangeStart, rangeEnd));
                StepLanes[step.Id] = assignedLane;
            }

            GroupLaneCount[group.PersonName] = Math.Max(1, lanes.Count);
        }
    }

    /// <summary>
    /// Gets the percentage position of a date on the timeline.
    /// </summary>
    public double GetPosition(DateTime date)
    {
        var days = (date - TimelineStart).TotalDays;
        return Math.Clamp(days / TimelineDays * 100, 0, 100);
    }

    public class PersonStepGroup
    {
        public string PersonName { get; set; } = string.Empty;
        public List<StepInstance> Steps { get; set; } = [];
    }

    public class ResolvedMilestone
    {
        public string Label { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public MilestoneColor Color { get; set; }
        public int? InstanceMilestoneId { get; set; }
    }
}
