using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleDrive.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Size",
                table: "BlobMetadata",
                newName: "size");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "BlobMetadata",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StorageType",
                table: "BlobMetadata",
                newName: "storage_type");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "BlobMetadata",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BlobId",
                table: "BlobMetadata",
                newName: "blob_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "size",
                table: "BlobMetadata",
                newName: "Size");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "BlobMetadata",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "storage_type",
                table: "BlobMetadata",
                newName: "StorageType");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "BlobMetadata",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "blob_id",
                table: "BlobMetadata",
                newName: "BlobId");
        }
    }
}
