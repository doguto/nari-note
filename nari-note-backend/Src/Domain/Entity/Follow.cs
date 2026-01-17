using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Entity;

[Index(nameof(FollowerId), nameof(FollowingId), IsUnique = true)]
public class Follow : EntityBase
{
    [Key]
    public FollowId Id { get; set; }

    [Required]
    [ForeignKey("Follower")]
    public UserId FollowerId { get; set; }

    [Required]
    [ForeignKey("Following")]
    public UserId FollowingId { get; set; }

    // Navigation Properties
    public User Follower { get; set; }  // フォローする側
    public User Following { get; set; }  // フォローされる側
}
