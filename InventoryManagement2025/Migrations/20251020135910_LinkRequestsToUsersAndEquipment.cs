using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement2025.Migrations
{
    /// <inheritdoc />
    public partial class LinkRequestsToUsersAndEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRequests_ApprovedById",
                table: "EquipmentRequests",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRequests_EquipmentId",
                table: "EquipmentRequests",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRequests_RequesterId",
                table: "EquipmentRequests",
                column: "RequesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentRequests_AspNetUsers_ApprovedById",
                table: "EquipmentRequests",
                column: "ApprovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentRequests_AspNetUsers_RequesterId",
                table: "EquipmentRequests",
                column: "RequesterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentRequests_Equipment_EquipmentId",
                table: "EquipmentRequests",
                column: "EquipmentId",
                principalTable: "Equipment",
                principalColumn: "EquipmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentRequests_AspNetUsers_ApprovedById",
                table: "EquipmentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentRequests_AspNetUsers_RequesterId",
                table: "EquipmentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentRequests_Equipment_EquipmentId",
                table: "EquipmentRequests");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentRequests_ApprovedById",
                table: "EquipmentRequests");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentRequests_EquipmentId",
                table: "EquipmentRequests");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentRequests_RequesterId",
                table: "EquipmentRequests");
        }
    }
}
