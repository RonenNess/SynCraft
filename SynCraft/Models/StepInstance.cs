using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SynCraft.Models;

public class StepInstance
{
    public int Id { get; set; }

    public int ProcessInstanceId { get; set; }

    [ForeignKey(nameof(ProcessInstanceId))]
    public ProcessInstance? ProcessInstance { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public StepCategory Category { get; set; }

    public int? ResponsiblePersonId { get; set; }

    [ForeignKey(nameof(ResponsiblePersonId))]
    public Person? ResponsiblePerson { get; set; }

    /// <summary>
    /// Offset in days from the target date.
    /// </summary>
    public int DayOffset { get; set; }

    /// <summary>
    /// Minimum duration in days needed to complete this step.
    /// </summary>
    public int MinDurationDays { get; set; }

    public StepState State { get; set; } = StepState.NotStarted;

    public List<StepComment> Comments { get; set; } = [];

    /// <summary>
    /// The deadline date, computed from the process instance's target date + day offset.
    /// </summary>
    [NotMapped]
    public DateTime Deadline => ProcessInstance != null
        ? ProcessInstance.TargetDate.AddDays(DayOffset)
        : DateTime.MinValue;

    /// <summary>
    /// The date by which this step must start to finish on time,
    /// computed as Deadline - MinDurationDays.
    /// </summary>
    [NotMapped]
    public DateTime MustStartBy => MinDurationDays > 0
        ? Deadline.AddDays(-MinDurationDays)
        : Deadline;

    /// <summary>
    /// Whether this step is overdue (past deadline and not completed/cancelled).
    /// </summary>
    [NotMapped]
    public bool IsDelayed => DateTime.Today > Deadline.Date
        && State != StepState.Done
        && State != StepState.Cancelled;

    /// <summary>
    /// Whether this step should have started by now but hasn't.
    /// </summary>
    [NotMapped]
    public bool ShouldHaveStarted => DateTime.Today >= MustStartBy.Date
        && State == StepState.NotStarted;

    /// <summary>
    /// Whether this step is in progress but doesn't have enough days
    /// remaining before the deadline to complete the required duration.
    /// </summary>
    [NotMapped]
    public bool IsAtRisk => State == StepState.InProgress
        && MinDurationDays > 0
        && (Deadline.Date - DateTime.Today).Days < MinDurationDays;
}
