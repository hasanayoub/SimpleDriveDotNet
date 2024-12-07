using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SimpleDrive.Models;

[Index(nameof(BlobId), IsUnique = true)]
public class BlobData
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    [Column(name: "id")]
    public long Id { get; init; }

    [MaxLength(64)]
    [Column(name: "blob_id")]
    [Required]
    public string BlobId { get; init; } = string.Empty;
    
    [MaxLength(20)]
    [Column(name: "content_type")]
    public string ContentType { get; init; } = string.Empty;
    
    [Column(name: "medium_blob_data", TypeName = "MEDIUMBLOB")]
    public byte[]? MediumBlobData { get; init; }
}