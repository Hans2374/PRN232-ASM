using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232_FA25_Assignment_G7.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class MakeViolationSubmissionNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SubmissionId",
                table: "Violations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                column: "PasswordHash",
                value: "$2a$11$MyugCFL2K4dU5UsMZZOr4eaIZIQFi2NQbgRCerwLe.CY40gqrXdHS");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e"),
                column: "PasswordHash",
                value: "$2a$11$qJY6nC/Fw5vf7w6V6pkEcOw4uzivLWDNyuzeuqgDkDpzzr3TBQodK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SubmissionId",
                table: "Violations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                column: "PasswordHash",
                value: "$2a$11$LxDGkUoRx4DxoLRiFKMg6us1Ge0lFXKznNnacLZl/QJ26Qb0zezGi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e"),
                column: "PasswordHash",
                value: "$2a$11$TjW2VArrKyYK293kxAQXDOqiascAtdeQdPmNKvtzXoRggkV8XfW3K");
        }
    }
}
