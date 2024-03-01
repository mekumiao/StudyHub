using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyHubDb.Migrations {
    /// <inheritdoc />
    public partial class AddFieldAnalysisToTopic : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                name: "Analysis",
                table: "Topics",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TopicSubjects_Name",
                table: "TopicSubjects",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropIndex(
                name: "IX_TopicSubjects_Name",
                table: "TopicSubjects");

            migrationBuilder.DropColumn(
                name: "Analysis",
                table: "Topics");
        }
    }
}
