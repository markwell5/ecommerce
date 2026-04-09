using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Return.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExchangeOrderId",
                table: "ReturnRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ExchangeProductId",
                table: "ReturnRequests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExchangeProductName",
                table: "ReturnRequests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_ExchangeOrderId",
                table: "ReturnRequests",
                column: "ExchangeOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReturnRequests_ExchangeOrderId",
                table: "ReturnRequests");

            migrationBuilder.DropColumn(
                name: "ExchangeOrderId",
                table: "ReturnRequests");

            migrationBuilder.DropColumn(
                name: "ExchangeProductId",
                table: "ReturnRequests");

            migrationBuilder.DropColumn(
                name: "ExchangeProductName",
                table: "ReturnRequests");
        }
    }
}
