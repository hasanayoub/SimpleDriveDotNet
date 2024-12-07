using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleDrive.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToBlobId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BlobMetadata_blob_id",
                table: "BlobMetadata",
                column: "blob_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BlobMetadata_blob_id",
                table: "BlobMetadata");
        }
    }
}
