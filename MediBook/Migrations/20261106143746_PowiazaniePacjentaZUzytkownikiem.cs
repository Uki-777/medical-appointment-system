using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediBook.Migrations
{
    /// <inheritdoc />
    public partial class PowiazaniePacjentaZUzytkownikiem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UzytkownikId",
                table: "Pacjenci",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pacjenci_UzytkownikId",
                table: "Pacjenci",
                column: "UzytkownikId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pacjenci_AspNetUsers_UzytkownikId",
                table: "Pacjenci",
                column: "UzytkownikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pacjenci_AspNetUsers_UzytkownikId",
                table: "Pacjenci");

            migrationBuilder.DropIndex(
                name: "IX_Pacjenci_UzytkownikId",
                table: "Pacjenci");

            migrationBuilder.DropColumn(
                name: "UzytkownikId",
                table: "Pacjenci");
        }
    }
}
