using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleDrive.Migrations
{
    /// <inheritdoc />
    public partial class AddMediumBlobToBlobMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "MediumBlobData",
                table: "BlobMetadata",
                type: "MEDIUMBLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediumBlobData",
                table: "BlobMetadata");
        }
    }
}
