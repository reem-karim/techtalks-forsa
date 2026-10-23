using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace techtalks_forsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixConcurrencyAndDuplicates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Key-like columns: nvarchar(max) -> nvarchar(450) so they can be indexed.
            migrationBuilder.AlterColumn<string>(
                name: "OwnerUserId",
                table: "Listings",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "Messages",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverId",
                table: "Messages",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Profiles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RequesterUserId",
                table: "SwapRequests",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // 2. New columns.
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Listings",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Profiles",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SwapRequests",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "RespondedAt",
                table: "SwapRequests",
                type: "datetime2",
                nullable: true);

            // 3. Clean up duplicates that already exist, otherwise the unique indexes below cannot be created.

            // 3a. Duplicate profiles for one user: keep the oldest, move minor profiles onto it,
            //     keep the guardian flag if any duplicate had it, delete the rest.
            migrationBuilder.Sql(@"
UPDATE m SET GuardianProfileId = k.KeepId
FROM MinorProfiles m
JOIN Profiles p ON p.Id = m.GuardianProfileId
JOIN (SELECT UserId, MIN(Id) AS KeepId FROM Profiles GROUP BY UserId) k ON k.UserId = p.UserId
WHERE m.GuardianProfileId <> k.KeepId;");

            migrationBuilder.Sql(@"
UPDATE keep SET IsGuardian = 1
FROM Profiles keep
JOIN (SELECT UserId, MIN(Id) AS KeepId FROM Profiles GROUP BY UserId) k ON k.KeepId = keep.Id
WHERE keep.IsGuardian = 0
  AND EXISTS (SELECT 1 FROM Profiles d WHERE d.UserId = keep.UserId AND d.IsGuardian = 1);");

            migrationBuilder.Sql(@"
DELETE p
FROM Profiles p
JOIN (SELECT UserId, MIN(Id) AS KeepId FROM Profiles GROUP BY UserId) k ON k.UserId = p.UserId
WHERE p.Id <> k.KeepId;");

            // 3b. Duplicate OPEN (Pending=0 / Accepted=1) requests for the same requester + listing:
            //     keep one (prefer an Accepted one, then the oldest) and mark the others Declined. Nothing is deleted.
            migrationBuilder.Sql(@"
WITH ranked AS (
    SELECT Id,
           ROW_NUMBER() OVER (
               PARTITION BY RequesterUserId, TargetListingId
               ORDER BY CASE WHEN Status = 1 THEN 0 ELSE 1 END, Id) AS rn
    FROM SwapRequests
    WHERE Status IN (0, 1))
UPDATE SwapRequests SET Status = 2, RespondedAt = SYSUTCDATETIME()
WHERE Id IN (SELECT Id FROM ranked WHERE rn > 1);");

            // 4. Indexes.
            migrationBuilder.DropIndex(
                name: "IX_Sessions_SwapRequestId",
                table: "Sessions");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SwapRequestId",
                table: "Sessions",
                column: "SwapRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_UserId",
                table: "Profiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Listings_OwnerUserId",
                table: "Listings",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId_ReceiverId",
                table: "Messages",
                columns: new[] { "SenderId", "ReceiverId" });

            migrationBuilder.CreateIndex(
                name: "IX_SwapRequests_RequesterUserId",
                table: "SwapRequests",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SwapRequests_RequesterUserId_TargetListingId",
                table: "SwapRequests",
                columns: new[] { "RequesterUserId", "TargetListingId" },
                unique: true,
                filter: "[Status] IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SwapRequests_RequesterUserId_TargetListingId",
                table: "SwapRequests");

            migrationBuilder.DropIndex(
                name: "IX_SwapRequests_RequesterUserId",
                table: "SwapRequests");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderId_ReceiverId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Listings_OwnerUserId",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_UserId",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_SwapRequestId",
                table: "Sessions");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SwapRequestId",
                table: "Sessions",
                column: "SwapRequestId");

            migrationBuilder.DropColumn(name: "RespondedAt", table: "SwapRequests");
            migrationBuilder.DropColumn(name: "RowVersion", table: "SwapRequests");
            migrationBuilder.DropColumn(name: "RowVersion", table: "Profiles");
            migrationBuilder.DropColumn(name: "RowVersion", table: "Listings");

            migrationBuilder.AlterColumn<string>(
                name: "RequesterUserId",
                table: "SwapRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverId",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerUserId",
                table: "Listings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);
        }
    }
}
