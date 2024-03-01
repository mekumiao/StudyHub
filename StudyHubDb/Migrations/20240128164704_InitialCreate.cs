using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyHubDb.Migrations {
    /// <inheritdoc />
    public partial class InitialCreate : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "AnswerRecords",
                columns: table => new {
                    AnswerRecordId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DifficultyLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SubmissionTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DurationSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeTakenSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSubmission = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsTimeout = table.Column<bool>(type: "INTEGER", nullable: false),
                    TotalTopic = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAnswer = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalCorrect = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalIncorrect = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_AnswerRecords", x => x.AnswerRecordId);
                });

            migrationBuilder.CreateTable(
                name: "TopicSubjects",
                columns: table => new {
                    TopicSubjectId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_TopicSubjects", x => x.TopicSubjectId);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new {
                    TopicId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TopicType = table.Column<int>(type: "INTEGER", nullable: false),
                    TopicBankFlag = table.Column<int>(type: "INTEGER", nullable: false),
                    DifficultyLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    TopicText = table.Column<string>(type: "TEXT", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "TEXT", nullable: false),
                    TopicSubjectId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Topics", x => x.TopicId);
                    table.ForeignKey(
                        name: "FK_Topics_TopicSubjects_TopicSubjectId",
                        column: x => x.TopicSubjectId,
                        principalTable: "TopicSubjects",
                        principalColumn: "TopicSubjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnswerRecordItems",
                columns: table => new {
                    AnswerRecordItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    TopicId = table.Column<int>(type: "INTEGER", nullable: true),
                    TopicType = table.Column<int>(type: "INTEGER", nullable: false),
                    DifficultyLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    TopicText = table.Column<string>(type: "TEXT", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "TEXT", nullable: false),
                    AnswerRecordId = table.Column<int>(type: "INTEGER", nullable: false),
                    AnswerText = table.Column<string>(type: "TEXT", nullable: true),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_AnswerRecordItems", x => x.AnswerRecordItemId);
                    table.ForeignKey(
                        name: "FK_AnswerRecordItems_AnswerRecords_AnswerRecordId",
                        column: x => x.AnswerRecordId,
                        principalTable: "AnswerRecords",
                        principalColumn: "AnswerRecordId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnswerRecordItems_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TopicOptions",
                columns: table => new {
                    TopicOptionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TopicId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<char>(type: "TEXT", nullable: false, defaultValue: 'A'),
                    Text = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_TopicOptions", x => x.TopicOptionId);
                    table.ForeignKey(
                        name: "FK_TopicOptions_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecordItems_AnswerRecordId",
                table: "AnswerRecordItems",
                column: "AnswerRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecordItems_TopicId",
                table: "AnswerRecordItems",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicOptions_TopicId",
                table: "TopicOptions",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_TopicSubjectId",
                table: "Topics",
                column: "TopicSubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "AnswerRecordItems");

            migrationBuilder.DropTable(
                name: "TopicOptions");

            migrationBuilder.DropTable(
                name: "AnswerRecords");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "TopicSubjects");
        }
    }
}
