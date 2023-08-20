using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce1.Migrations.Resource
{
    public partial class MidAugustBug1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAddresses_WarehouseAddresses_AddressId",
                table: "ProductAddresses");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAddresses_WarehouseAddresses_AddressId",
                table: "ProductAddresses",
                column: "AddressId",
                principalTable: "WarehouseAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAddresses_WarehouseAddresses_AddressId",
                table: "ProductAddresses");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAddresses_WarehouseAddresses_AddressId",
                table: "ProductAddresses",
                column: "AddressId",
                principalTable: "WarehouseAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
