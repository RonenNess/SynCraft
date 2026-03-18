using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Timeline;

public class IndexModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public IndexModel(SynCraftDbContext db) => _db = db;

    public List<DynamicTimelineStep> Steps { get; set; } = [];
    public List<DynamicTimelineMilestone> Milestones { get; set; } = [];
    public List<PersonGroup> StepGroups { get; set; } = [];
    public Dictionary<int, int> StepLanes { get; set; } = [];
    public Dictionary<string, int> GroupLaneCount { get; set; } = [];
    public List<string> KnownPersons { get; set; } = [];
    public List<ProcessRow> ProcessRows { get; set; } = [];

    public const int LaneHeight = 50;

    public DateTime TimelineStart { get; set; }
    public DateTime TimelineEnd { get; set; }
    public int TimelineDays { get; set; }

    public async Task OnGetAsync()
    {
        Steps = await _db.DynamicTimelineSteps.OrderBy(s => s.EndDate).ToListAsync();
        Milestones = await _db.DynamicTimelineMilestones.OrderBy(m => m.Date).ToListAsync();
        KnownPersons = await _db.Persons.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync();

        var today = DateTime.Today;
        var processes = await _db.ProcessInstances
            .Include(p => p.Steps)
            .Where(p => p.Status == ProcessStatus.Active || p.TargetDate >= today)
            .OrderBy(p => p.CreatedDate)
            .ToListAsync();

        ProcessRows = processes.Select(p =>
        {
            var stepsDone = p.Steps.Count(s => s.State == StepState.Done || s.State == StepState.Cancelled);
            var stepsTotal = p.Steps.Count;
            var oldestStepDate = p.Steps.Count > 0
                ? p.Steps.Min(s => s.MustStartBy)
                : p.CreatedDate.Date;
            return new ProcessRow
            {
                Id = p.Id,
                Name = p.Name,
                StartDate = oldestStepDate,
                EndDate = p.TargetDate.Date,
                Status = p.Status,
                StepsDone = stepsDone,
                StepsTotal = stepsTotal
            };
        }).ToList();

        BuildTimeline();

        foreach (var pr in ProcessRows)
        {
            pr.IsCutStart = pr.StartDate < TimelineStart;
            pr.IsCutEnd = pr.EndDate > TimelineEnd;
        }
    }

    public async Task<IActionResult> OnPostAddStepAsync(
        string name, string? description, string assigneeName,
        StepCategory category, DateTime startDate, DateTime endDate, StepState state)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(assigneeName))
            return RedirectToPage();

        if (endDate < startDate)
            endDate = startDate;

        _db.DynamicTimelineSteps.Add(new DynamicTimelineStep
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            AssigneeName = assigneeName.Trim(),
            Category = category,
            StartDate = startDate,
            EndDate = endDate,
            State = state
        });
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditStepAsync(
        int stepId, string name, string? description, string assigneeName,
        StepCategory category, DateTime startDate, DateTime endDate, StepState state)
    {
        var step = await _db.DynamicTimelineSteps.FindAsync(stepId);
        if (step == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(name)) step.Name = name.Trim();
        if (!string.IsNullOrWhiteSpace(assigneeName)) step.AssigneeName = assigneeName.Trim();
        step.Description = description?.Trim();
        step.Category = category;
        step.StartDate = startDate;
        step.EndDate = endDate < startDate ? startDate : endDate;
        step.State = state;
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteStepAsync(int stepId)
    {
        var step = await _db.DynamicTimelineSteps.FindAsync(stepId);
        if (step != null)
        {
            _db.DynamicTimelineSteps.Remove(step);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddMilestoneAsync(string label, DateTime date, MilestoneColor color)
    {
        if (string.IsNullOrWhiteSpace(label))
            return RedirectToPage();

        _db.DynamicTimelineMilestones.Add(new DynamicTimelineMilestone
        {
            Label = label.Trim(),
            Date = date,
            Color = color
        });
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditMilestoneAsync(
        int milestoneId, string label, DateTime date, MilestoneColor color)
    {
        var ms = await _db.DynamicTimelineMilestones.FindAsync(milestoneId);
        if (ms == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(label)) ms.Label = label.Trim();
        ms.Date = date;
        ms.Color = color;
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteMilestoneAsync(int milestoneId)
    {
        var ms = await _db.DynamicTimelineMilestones.FindAsync(milestoneId);
        if (ms != null)
        {
            _db.DynamicTimelineMilestones.Remove(ms);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCleanupAsync()
    {
        var today = DateTime.Today;

        // Remove completed/cancelled steps whose end date is in the past
        var oldSteps = await _db.DynamicTimelineSteps
            .Where(s => s.EndDate < today &&
                (s.State == StepState.Done || s.State == StepState.Cancelled))
            .ToListAsync();
        _db.DynamicTimelineSteps.RemoveRange(oldSteps);

        // Remove milestones whose date is in the past
        var oldMilestones = await _db.DynamicTimelineMilestones
            .Where(m => m.Date < today)
            .ToListAsync();
        _db.DynamicTimelineMilestones.RemoveRange(oldMilestones);

        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    private void BuildTimeline()
    {
        var today = DateTime.Today;
        var openSteps = Steps.Where(s => s.State != StepState.Done && s.State != StepState.Cancelled).ToList();

        var allDates = new List<DateTime> { today };

        foreach (var s in Steps)
        {
            allDates.Add(s.StartDate);
            allDates.Add(s.EndDate);
        }
        foreach (var m in Milestones)
            allDates.Add(m.Date);

        if (openSteps.Count > 0 || ProcessRows.Count > 0)
        {
            TimelineStart = allDates.Min().AddDays(-7);
            TimelineEnd = allDates.Max().AddDays(7);
        }
        else
        {
            allDates.Add(today.AddMonths(-1));
            allDates.Add(today.AddMonths(1));
            TimelineStart = allDates.Min().AddDays(-7);
            TimelineEnd = allDates.Max().AddDays(7);
        }

        TimelineDays = Math.Max(1, (TimelineEnd - TimelineStart).Days);

        StepGroups = Steps
            .GroupBy(s => s.AssigneeName)
            .OrderBy(g => g.Key)
            .Select(g => new PersonGroup
            {
                PersonName = g.Key,
                Steps = g.OrderBy(s => s.EndDate).ToList()
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
                var endPos = GetPosition(step.EndDate);
                var startPos = GetPosition(step.StartDate);
                var rangeStart = Math.Min(startPos, endPos) - overlapThreshold;
                var rangeEnd = Math.Max(startPos, endPos) + overlapThreshold;

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

    public double GetPosition(DateTime date)
    {
        var days = (date - TimelineStart).TotalDays;
        return Math.Clamp(days / TimelineDays * 100, 0, 100);
    }

    public class PersonGroup
    {
        public string PersonName { get; set; } = string.Empty;
        public List<DynamicTimelineStep> Steps { get; set; } = [];
    }

    public class ProcessRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ProcessStatus Status { get; set; }
        public int StepsDone { get; set; }
        public int StepsTotal { get; set; }
        public bool IsCutStart { get; set; }
        public bool IsCutEnd { get; set; }
    }
}
