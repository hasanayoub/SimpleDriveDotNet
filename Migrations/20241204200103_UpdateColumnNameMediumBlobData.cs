using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleDrive.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnNameMediumBlobData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MediumBlobData",
                table: "BlobMetadata",
                newName: "medium_blob_data");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "medium_blob_data",
                table: "BlobMetadata",
                newName: "MediumBlobData");
        }
    }
}
