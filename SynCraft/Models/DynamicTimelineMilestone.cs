using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SynCraft.Models;

[Index(nameof(Date))]
public class DynamicTimelineMilestone
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Label { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    public MilestoneColor Color { get; set; } = MilestoneColor.Blue;
}
