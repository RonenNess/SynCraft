using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SynCraft.Models;

public class MilestoneTemplate
{
    public int Id { get; set; }

    public int ProcessTemplateId { get; set; }

    [ForeignKey(nameof(ProcessTemplateId))]
    public ProcessTemplate? ProcessTemplate { get; set; }

    [Required]
    [StringLength(200)]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Offset in days from the target date. Negative means before the target date.
    /// </summary>
    [Required]
    [Display(Name = "Day Offset")]
    public int DayOffset { get; set; }

    [Required]
    public MilestoneColor Color { get; set; } = MilestoneColor.Blue;
}
