using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SimpleDrive.Models;

[Index(nameof(BlobId), IsUnique = true)]
public class BlobMetadata
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    [Column(name: "id")]
    public long Id { get; init; }
    
    [MaxLength(64)]
    [Column(name: "blob_id")]
    [Required]
    public string BlobId { get; init; } = string.Empty;
    
    [Column(name: "storage_type")]
    [MaxLength(10)]
    public BlobStorageType StorageType { get; init; } = BlobStorageType.Database;
    
    [Column(name: "size")] public long Size { get; init; }
    
    [Column(name: "created_at")] public DateTime CreatedAt { get; init; }
    
    [MaxLength(10)]
    [Column(name: "content_type")]
    public string ContentType { get; init; } = string.Empty;
}