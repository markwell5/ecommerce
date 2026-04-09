using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Return.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnShipments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReturnShipments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReturnRequestId = table.Column<long>(type: "bigint", nullable: false),
                    Carrier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TrackingNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LabelUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DropOffLocation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ShippedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnShipments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnShipments_ReturnRequestId",
                table: "ReturnShipments",
                column: "ReturnRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReturnShipments_TrackingNumber",
                table: "ReturnShipments",
                column: "TrackingNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReturnShipments");
        }
    }
}
