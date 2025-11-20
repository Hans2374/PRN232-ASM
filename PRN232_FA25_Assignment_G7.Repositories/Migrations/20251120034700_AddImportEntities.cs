using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232_FA25_Assignment_G7.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddImportEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsZeroScore",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "ReviewComments",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Violations");

            migrationBuilder.RenameColumn(
                name: "ReviewStatus",
                table: "Violations",
                newName: "ViolationType");

            migrationBuilder.AlterColumn<int>(
                name: "Severity",
                table: "Violations",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "ConfidenceScore",
                table: "Violations",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Violations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Evidence",
                table: "Violations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Violations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DuplicateGroupId",
                table: "Submissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedText",
                table: "Submissions",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "Submissions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImportJobId",
                table: "Submissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DuplicateGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SimilarityScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuplicateGroups_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImportJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArchiveName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SubjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SemesterCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UploaderUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalFiles = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ProcessedFiles = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SuccessCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FailedCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ViolationsCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StorageFolderPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportJobs_Users_UploaderUserId",
                        column: x => x.UploaderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionImages_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Violations_CreatedBy",
                table: "Violations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_DuplicateGroupId",
                table: "Submissions",
                column: "DuplicateGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ImportJobId",
                table: "Submissions",
                column: "ImportJobId");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateGroups_ExamId",
                table: "DuplicateGroups",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_UploaderUserId",
                table: "ImportJobs",
                column: "UploaderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionImages_SubmissionId",
                table: "SubmissionImages",
                column: "SubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_DuplicateGroups_DuplicateGroupId",
                table: "Submissions",
                column: "DuplicateGroupId",
                principalTable: "DuplicateGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_ImportJobs_ImportJobId",
                table: "Submissions",
                column: "ImportJobId",
                principalTable: "ImportJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Violations_Users_CreatedBy",
                table: "Violations",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_DuplicateGroups_DuplicateGroupId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_ImportJobs_ImportJobId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Violations_Users_CreatedBy",
                table: "Violations");

            migrationBuilder.DropTable(
                name: "DuplicateGroups");

            migrationBuilder.DropTable(
                name: "ImportJobs");

            migrationBuilder.DropTable(
                name: "SubmissionImages");

            migrationBuilder.DropIndex(
                name: "IX_Violations_CreatedBy",
                table: "Violations");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_DuplicateGroupId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_ImportJobId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "Evidence",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "DuplicateGroupId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "ExtractedText",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "ImportJobId",
                table: "Submissions");

            migrationBuilder.RenameColumn(
                name: "ViolationType",
                table: "Violations",
                newName: "ReviewStatus");

            migrationBuilder.AlterColumn<int>(
                name: "Severity",
                table: "Violations",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsZeroScore",
                table: "Violations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReviewComments",
                table: "Violations",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

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
                name: "Type",
                table: "Violations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                column: "PasswordHash",
                value: "$2a$11$hc459wRTrKyZ3IwOW0WF0.2S4Ngtv.PlcchU1owZcHr4XSvN2muyq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e"),
                column: "PasswordHash",
                value: "$2a$11$6QfirpMoyH7S92rhq/yqDOK2bUn8MM0XkgabPAMlt.xwAug5v7PHO");
        }
    }
}
