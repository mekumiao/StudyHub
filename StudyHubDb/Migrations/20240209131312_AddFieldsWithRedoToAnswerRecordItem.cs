using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyHubDb.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsWithRedoToAnswerRecordItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnswerRecordType",
                table: "AnswerRecordItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsRedoCorrectly",
                table: "AnswerRecordItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RedoFromAnswerRecordItemId",
                table: "AnswerRecordItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TopicSubjectId",
                table: "AnswerRecordItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerRecordType",
                table: "AnswerRecordItems");

            migrationBuilder.DropColumn(
                name: "IsRedoCorrectly",
                table: "AnswerRecordItems");

            migrationBuilder.DropColumn(
                name: "RedoFromAnswerRecordItemId",
                table: "AnswerRecordItems");

            migrationBuilder.DropColumn(
                name: "TopicSubjectId",
                table: "AnswerRecordItems");
        }
    }
}
