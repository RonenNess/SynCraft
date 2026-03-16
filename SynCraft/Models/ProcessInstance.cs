using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SynCraft.Models;

public class ProcessInstance
{
    public int Id { get; set; }

    public int ProcessTemplateId { get; set; }

    [ForeignKey(nameof(ProcessTemplateId))]
    public ProcessTemplate? ProcessTemplate { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Target Date")]
    [DataType(DataType.Date)]
    public DateTime TargetDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ProcessStatus Status { get; set; } = ProcessStatus.Active;

    public List<StepInstance> Steps { get; set; } = [];

    public List<MilestoneInstance> Milestones { get; set; } = [];

    public bool CanComplete => Steps.Count > 0 &&
        Steps.All(s => s.State == StepState.Done || s.State == StepState.Cancelled);
}
