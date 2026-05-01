using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargoAPI.DataAccess.Migrations
{
    public partial class AddCarrierReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarrierReports",
                columns: table => new
                {
                    CarrierReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarrierId = table.Column<int>(type: "int", nullable: false),
                    CarrierCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CarrierReportDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarrierReports", x => x.CarrierReportId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarrierReports_CarrierId_CarrierReportDate",
                table: "CarrierReports",
                columns: new[] { "CarrierId", "CarrierReportDate" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarrierReports");
        }
    }
}
