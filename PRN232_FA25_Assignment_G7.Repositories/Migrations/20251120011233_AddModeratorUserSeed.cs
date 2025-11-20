using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232_FA25_Assignment_G7.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddModeratorUserSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                column: "PasswordHash",
                value: "$2a$11$hc459wRTrKyZ3IwOW0WF0.2S4Ngtv.PlcchU1owZcHr4XSvN2muyq");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "Role", "Username" },
                values: new object[] { new Guid("b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "moderator@example.com", "Content Moderator", true, "$2a$11$6QfirpMoyH7S92rhq/yqDOK2bUn8MM0XkgabPAMlt.xwAug5v7PHO", 3, "moderator" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                column: "PasswordHash",
                value: "$2a$11$FcDijCyPBozXdoe4TxLvCugkyaJgI0PK9v.MzdiQDW0Ne.0V5nHV.");
        }
    }
}
