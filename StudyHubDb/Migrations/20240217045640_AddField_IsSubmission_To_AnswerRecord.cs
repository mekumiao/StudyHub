using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyHubDb.Migrations
{
    /// <inheritdoc />
    public partial class AddField_IsSubmission_To_AnswerRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSubmission",
                table: "AnswerRecordItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSubmission",
                table: "AnswerRecordItems");
        }
    }
}
