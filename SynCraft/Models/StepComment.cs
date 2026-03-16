using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SynCraft.Models;

public class StepComment
{
    public int Id { get; set; }

    public int StepInstanceId { get; set; }

    [ForeignKey(nameof(StepInstanceId))]
    public StepInstance? StepInstance { get; set; }

    [Required]
    [StringLength(100)]
    public string Author { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
