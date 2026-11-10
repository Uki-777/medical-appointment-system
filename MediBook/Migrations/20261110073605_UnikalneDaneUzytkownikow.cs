using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediBook.Migrations
{
    /// <inheritdoc />
    public partial class UnikalneDaneUzytkownikow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pacjenci_UzytkownikId",
                table: "Pacjenci");

            migrationBuilder.DropIndex(
                name: "IX_Lekarze_UzytkownikId",
                table: "Lekarze");

            migrationBuilder.CreateIndex(
                name: "IX_Pacjenci_Email",
                table: "Pacjenci",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pacjenci_UzytkownikId",
                table: "Pacjenci",
                column: "UzytkownikId",
                unique: true,
                filter: "[UzytkownikId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lekarze_Email",
                table: "Lekarze",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lekarze_UzytkownikId",
                table: "Lekarze",
                column: "UzytkownikId",
                unique: true,
                filter: "[UzytkownikId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pacjenci_Email",
                table: "Pacjenci");

            migrationBuilder.DropIndex(
                name: "IX_Pacjenci_UzytkownikId",
                table: "Pacjenci");

            migrationBuilder.DropIndex(
                name: "IX_Lekarze_Email",
                table: "Lekarze");

            migrationBuilder.DropIndex(
                name: "IX_Lekarze_UzytkownikId",
                table: "Lekarze");

            migrationBuilder.CreateIndex(
                name: "IX_Pacjenci_UzytkownikId",
                table: "Pacjenci",
                column: "UzytkownikId");

            migrationBuilder.CreateIndex(
                name: "IX_Lekarze_UzytkownikId",
                table: "Lekarze",
                column: "UzytkownikId");
        }
    }
}
