using System.ComponentModel.DataAnnotations;

namespace SynCraft.Models;

public class ProcessTemplate
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(500)]
    [Display(Name = "Target Date Comment")]
    public string? TargetDateComment { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public List<StepTemplate> Steps { get; set; } = [];

    public List<MilestoneTemplate> Milestones { get; set; } = [];
}
