using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyHubDb.Migrations
{
    /// <inheritdoc />
    public partial class AddSomeFieldToAnswerRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "AnswerRecordItems");

            migrationBuilder.AddColumn<int>(
                name: "AnswerRecordType",
                table: "AnswerRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FillCount",
                table: "AnswerRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FillScore",
                table: "AnswerRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MultipleCount",
                table: "AnswerRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MultipleScore",
                table: "AnswerRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "AnswerRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SingleCount",
                table: "AnswerRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SingleScore",
                table: "AnswerRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalScore",
                table: "AnswerRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrueFalseCount",
                table: "AnswerRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrueFalseScore",
                table: "AnswerRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "TopicId",
                table: "AnswerRecordItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
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
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "FillCount",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "FillScore",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "MultipleCount",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "MultipleScore",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "SingleCount",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "SingleScore",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "TotalScore",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "TrueFalseCount",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "TrueFalseScore",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "State",
                table: "AnswerRecordItems");

            migrationBuilder.AlterColumn<int>(
                name: "TopicId",
                table: "AnswerRecordItems",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "AnswerRecordItems",
                type: "INTEGER",
                nullable: true);
        }
    }
}
