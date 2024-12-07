using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SimpleDrive.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BlobMetadata> BlobMetadata { get; init; } = null!;
    public DbSet<BlobData> BlobData { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlobMetadata>()
            .Property(b => b.StorageType)
            .HasConversion(new EnumToStringConverter<BlobStorageType>()); // Converts the enum to a string
    }
}