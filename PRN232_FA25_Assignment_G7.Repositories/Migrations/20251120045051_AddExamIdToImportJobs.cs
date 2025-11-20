using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232_FA25_Assignment_G7.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddExamIdToImportJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExamId",
                table: "ImportJobs",
                type: "uniqueidentifier",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_ExamId",
                table: "ImportJobs",
                column: "ExamId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportJobs_Exams_ExamId",
                table: "ImportJobs",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportJobs_Exams_ExamId",
                table: "ImportJobs");

            migrationBuilder.DropIndex(
                name: "IX_ImportJobs_ExamId",
                table: "ImportJobs");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "ImportJobs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                column: "PasswordHash",
                value: "$2a$11$6CfecY.D4UYWAggFoLbj3.eG/ZyaZ89Yt4n4NFxeJ/FSksNOVRbGy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e"),
                column: "PasswordHash",
                value: "$2a$11$bDbE47mZvsTIlOHpFb5k7O5Rr3a9fgkmEsWzP84m8I.9dZ1.jSG7i");
        }
    }
}
