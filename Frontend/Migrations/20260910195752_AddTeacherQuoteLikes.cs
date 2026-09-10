using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frontend.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherQuoteLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeacherQuoteLikes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherQuoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherQuoteLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherQuoteLikes_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherQuoteLikes_TeacherQuotes_TeacherQuoteId",
                        column: x => x.TeacherQuoteId,
                        principalTable: "TeacherQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherQuoteLikes_StudentId",
                table: "TeacherQuoteLikes",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherQuoteLikes_TeacherQuoteId_StudentId",
                table: "TeacherQuoteLikes",
                columns: new[] { "TeacherQuoteId", "StudentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherQuoteLikes");
        }
    }
}
