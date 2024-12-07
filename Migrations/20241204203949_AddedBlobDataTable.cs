using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleDrive.Migrations
{
    /// <inheritdoc />
    public partial class AddedBlobDataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "medium_blob_data",
                table: "BlobMetadata");

            migrationBuilder.CreateTable(
                name: "BlobData",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    blob_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    medium_blob_data = table.Column<byte[]>(type: "MEDIUMBLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlobData", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BlobData_blob_id",
                table: "BlobData",
                column: "blob_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlobData");

            migrationBuilder.AddColumn<byte[]>(
                name: "medium_blob_data",
                table: "BlobMetadata",
                type: "MEDIUMBLOB",
                nullable: true);
        }
    }
}
