using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookSellingManagement.Migrations
{
    public partial class SecondMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportedQuantity",
                table: "Books");

            migrationBuilder.RenameColumn(
                name: "SoldQuantity",
                table: "Books",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Books",
                newName: "Decription");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Books",
                newName: "SoldQuantity");

            migrationBuilder.RenameColumn(
                name: "Decription",
                table: "Books",
                newName: "Description");

            migrationBuilder.AddColumn<int>(
                name: "ImportedQuantity",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
