using System.ComponentModel.DataAnnotations;

namespace SynCraft.Models;

public class Person
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(100)]
    public string? Role { get; set; }
}
