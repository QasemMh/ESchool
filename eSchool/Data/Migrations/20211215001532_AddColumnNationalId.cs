using Microsoft.EntityFrameworkCore.Migrations;

namespace eSchool.Data.Migrations
{
    public partial class AddColumnNationalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "Teachers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "Students",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "Parents",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_NationalId",
                table: "Teachers",
                column: "NationalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_NationalId",
                table: "Students",
                column: "NationalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parents_NationalId",
                table: "Parents",
                column: "NationalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teachers_NationalId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_NationalId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Parents_NationalId",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "Parents");
        }
    }
}
