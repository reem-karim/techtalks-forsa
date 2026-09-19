using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace techtalks_forsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGuardianAndMinorProfilee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGuardian",
                table: "Profiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MinorProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuardianProfileId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MinorProfiles_Profiles_GuardianProfileId",
                        column: x => x.GuardianProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MinorProfiles_GuardianProfileId",
                table: "MinorProfiles",
                column: "GuardianProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MinorProfiles");

            migrationBuilder.DropColumn(
                name: "IsGuardian",
                table: "Profiles");
        }
    }
}