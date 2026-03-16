using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SynCraft.Models;

public class StepTemplate
{
    public int Id { get; set; }

    public int ProcessTemplateId { get; set; }

    [ForeignKey(nameof(ProcessTemplateId))]
    public ProcessTemplate? ProcessTemplate { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public StepCategory Category { get; set; }

    public int? ResponsiblePersonId { get; set; }

    [ForeignKey(nameof(ResponsiblePersonId))]
    public Person? ResponsiblePerson { get; set; }

    /// <summary>
    /// Offset in days from the target date. Negative means before the target date.
    /// For example, -7 means 7 days before the target date.
    /// </summary>
    [Required]
    [Display(Name = "Day Offset")]
    public int DayOffset { get; set; }

    /// <summary>
    /// Minimum duration in days needed to complete this step.
    /// Used to determine when the step must start.
    /// </summary>
    [Display(Name = "Min Duration (days)")]
    public int MinDurationDays { get; set; }
}
