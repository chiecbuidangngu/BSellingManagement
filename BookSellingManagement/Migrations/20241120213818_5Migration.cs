using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookSellingManagement.Migrations
{
    public partial class _5Migration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderId",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "OrderDetails");
        }
    }
}
