using Microsoft.EntityFrameworkCore.Migrations;

namespace eSchool.Data.Migrations
{
    public partial class ChangeFKRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Addresses");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Addresses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
