using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232_FA25_Assignment_G7.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowStatusAndExamExaminer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Violations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsZeroScore",
                table: "Violations",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Violations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Violations",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "ReviewComments",
                table: "Violations",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewStatus",
                table: "Violations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Violations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedBy",
                table: "Violations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminComments",
                table: "Submissions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GradedAt",
                table: "Submissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GradedBy",
                table: "Submissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradingComments",
                table: "Submissions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModeratorComments",
                table: "Submissions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewStatus",
                table: "Submissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SecondGradedAt",
                table: "Submissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecondGradedBy",
                table: "Submissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondGradingComments",
                table: "Submissions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SecondScore",
                table: "Submissions",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubmissionStatus",
                table: "Submissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ExamExaminers",
                columns: table => new
                {
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExaminerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPrimaryGrader = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamExaminers", x => new { x.ExamId, x.ExaminerId });
                    table.ForeignKey(
                        name: "FK_ExamExaminers_Examiners_ExaminerId",
                        column: x => x.ExaminerId,
                        principalTable: "Examiners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamExaminers_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                column: "PasswordHash",
                value: "$2a$11$NN7s91WhpREHokf0kwgo7.mIMf4DcwyHamCktzPzrlDFbgGI0Cmny");

            migrationBuilder.CreateIndex(
                name: "IX_ExamExaminers_ExaminerId",
                table: "ExamExaminers",
                column: "ExaminerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamExaminers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "ReviewComments",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "AdminComments",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "GradedAt",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "GradedBy",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "GradingComments",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "ModeratorComments",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SecondGradedAt",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SecondGradedBy",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SecondGradingComments",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SecondScore",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SubmissionStatus",
                table: "Submissions");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Violations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsZeroScore",
                table: "Violations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Violations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                column: "PasswordHash",
                value: "$2a$11$Vh.XvSWloAUNlKfoNmNSHOl0QfR9SNokr/gcB98Dp.bHBH9VrVOUy");
        }
    }
}
