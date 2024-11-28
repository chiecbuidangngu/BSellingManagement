using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookSellingManagement.Migrations
{
    public partial class úpumbook : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SumBook",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SumBook",
                table: "Authors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SumBook",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "SumBook",
                table: "Authors");
        }
    }
}
