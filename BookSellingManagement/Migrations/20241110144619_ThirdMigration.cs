using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookSellingManagement.Migrations
{
    public partial class ThirdMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Books",
                newName: "ImportedQuantity");

            migrationBuilder.RenameColumn(
                name: "Decription",
                table: "Books",
                newName: "Description");

            migrationBuilder.AddColumn<int>(
                name: "SoldQuantity",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoldQuantity",
                table: "Books");

            migrationBuilder.RenameColumn(
                name: "ImportedQuantity",
                table: "Books",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Books",
                newName: "Decription");
        }
    }
}
