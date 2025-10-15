using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement2025.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestFieldsAndLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "End",
                table: "EquipmentRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnNotes",
                table: "EquipmentRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnedAt",
                table: "EquipmentRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Start",
                table: "EquipmentRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSensitive",
                table: "Equipment",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ConditionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    OldCondition = table.Column<int>(type: "INTEGER", nullable: false),
                    NewCondition = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChangedById = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConditionLogs");

            migrationBuilder.DropColumn(
                name: "End",
                table: "EquipmentRequests");

            migrationBuilder.DropColumn(
                name: "ReturnNotes",
                table: "EquipmentRequests");

            migrationBuilder.DropColumn(
                name: "ReturnedAt",
                table: "EquipmentRequests");

            migrationBuilder.DropColumn(
                name: "Start",
                table: "EquipmentRequests");

            migrationBuilder.DropColumn(
                name: "IsSensitive",
                table: "Equipment");
        }
    }
}
