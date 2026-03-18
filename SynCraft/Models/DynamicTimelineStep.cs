using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SynCraft.Models;

[Index(nameof(AssigneeName))]
[Index(nameof(EndDate))]
[Index(nameof(State))]
public class DynamicTimelineStep
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(100)]
    public string AssigneeName { get; set; } = string.Empty;

    public StepCategory Category { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public StepState State { get; set; } = StepState.NotStarted;
}
