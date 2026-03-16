using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SynCraft.Models;

public class MilestoneInstance
{
    public int Id { get; set; }

    public int ProcessInstanceId { get; set; }

    [ForeignKey(nameof(ProcessInstanceId))]
    public ProcessInstance? ProcessInstance { get; set; }

    [Required]
    [StringLength(200)]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The concrete date of this milestone.
    /// </summary>
    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    public MilestoneColor Color { get; set; } = MilestoneColor.Blue;
}
