using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NariNoteBackend.Domain;

[Index(nameof(FollowerId), nameof(FollowingId), IsUnique = true)]
public class Follow : EntityBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Follower")]
    public int FollowerId { get; set; }

    [Required]
    [ForeignKey("Following")]
    public int FollowingId { get; set; }

    // Navigation Properties
    public required User Follower { get; set; }  // フォローする側
    public required User Following { get; set; }  // フォローされる側
}
