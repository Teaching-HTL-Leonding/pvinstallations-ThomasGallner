﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pv_api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PvInstallations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Longitude = table.Column<float>(type: "real", nullable: false),
                    Latitude = table.Column<float>(type: "real", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PvInstallations", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ProductionReports",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProducedWattage = table.Column<float>(type: "real", nullable: false),
                    HouseholdWattage = table.Column<float>(type: "real", nullable: false),
                    BatteryWattage = table.Column<float>(type: "real", nullable: false),
                    GridWattage = table.Column<float>(type: "real", nullable: false),
                    PvInstallationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionReports", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProductionReports_PvInstallations_PvInstallationId",
                        column: x => x.PvInstallationId,
                        principalTable: "PvInstallations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReports_PvInstallationId",
                table: "ProductionReports",
                column: "PvInstallationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionReports");

            migrationBuilder.DropTable(
                name: "PvInstallations");
        }
    }
}
