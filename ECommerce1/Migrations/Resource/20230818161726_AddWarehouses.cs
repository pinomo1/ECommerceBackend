using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce1.Migrations.Resource
{
    public partial class AddWarehouses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InStock",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WarehouseAddressCopy",
                table: "Orders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "CartItems",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "RecentlyViewedItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecentlyViewedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecentlyViewedItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecentlyViewedItems_Profiles_UserId",
                        column: x => x.UserId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    First = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Second = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Zip = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseAddresses_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehouseAddresses_Sellers_UserId",
                        column: x => x.UserId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAddresses_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductAddresses_WarehouseAddresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "WarehouseAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAddresses_AddressId",
                table: "ProductAddresses",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAddresses_ProductId",
                table: "ProductAddresses",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyViewedItems_ProductId",
                table: "RecentlyViewedItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyViewedItems_UserId",
                table: "RecentlyViewedItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseAddresses_CityId",
                table: "WarehouseAddresses",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseAddresses_UserId",
                table: "WarehouseAddresses",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAddresses");

            migrationBuilder.DropTable(
                name: "RecentlyViewedItems");

            migrationBuilder.DropTable(
                name: "WarehouseAddresses");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WarehouseAddressCopy",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "CartItems");

            migrationBuilder.AddColumn<bool>(
                name: "InStock",
                table: "Products",
                type: "bit",
                nullable: true,
                defaultValue: true);
        }
    }
}
