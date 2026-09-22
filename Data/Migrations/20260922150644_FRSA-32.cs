using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace techtalks_forsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class FRSA32 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SwapRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesterUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetListingId = table.Column<int>(type: "int", nullable: false),
                    OfferedListingId = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwapRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SwapRequests_Listings_OfferedListingId",
                        column: x => x.OfferedListingId,
                        principalTable: "Listings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SwapRequests_Listings_TargetListingId",
                        column: x => x.TargetListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SwapRequests_OfferedListingId",
                table: "SwapRequests",
                column: "OfferedListingId");

            migrationBuilder.CreateIndex(
                name: "IX_SwapRequests_TargetListingId",
                table: "SwapRequests",
                column: "TargetListingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SwapRequests");
        }
    }
}
