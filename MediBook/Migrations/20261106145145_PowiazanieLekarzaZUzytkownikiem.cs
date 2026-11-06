using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediBook.Migrations
{
    /// <inheritdoc />
    public partial class PowiazanieLekarzaZUzytkownikiem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Lekarze",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UzytkownikId",
                table: "Lekarze",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lekarze_UzytkownikId",
                table: "Lekarze",
                column: "UzytkownikId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lekarze_AspNetUsers_UzytkownikId",
                table: "Lekarze",
                column: "UzytkownikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lekarze_AspNetUsers_UzytkownikId",
                table: "Lekarze");

            migrationBuilder.DropIndex(
                name: "IX_Lekarze_UzytkownikId",
                table: "Lekarze");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Lekarze");

            migrationBuilder.DropColumn(
                name: "UzytkownikId",
                table: "Lekarze");
        }
    }
}
