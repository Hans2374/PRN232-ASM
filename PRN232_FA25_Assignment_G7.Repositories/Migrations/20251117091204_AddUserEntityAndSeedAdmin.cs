using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232_FA25_Assignment_G7.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEntityAndSeedAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "Role", "Username" },
                values: new object[] { new Guid("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@example.com", "System Administrator", true, "$2a$11$Vh.XvSWloAUNlKfoNmNSHOl0QfR9SNokr/gcB98Dp.bHBH9VrVOUy", 1, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
