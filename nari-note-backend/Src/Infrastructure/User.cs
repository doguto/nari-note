using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NariNoteBackend.Infrastructure;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    public string Name { get; set; } = "";

    [Column(TypeName = "varchar(255)")]
    public string ProfileImage { get; set; } = "";

    [Column(TypeName = "varchar(255)")]
    public string Email { get; set; } = "";

    [Column(TypeName = "varchar(255)")]
    public string PasswordHash { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
