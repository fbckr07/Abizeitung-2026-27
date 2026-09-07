using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frontend.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    SentBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ActiveUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDuo = table.Column<bool>(type: "boolean", nullable: false),
                    IsCrossGender = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OldId = table.Column<int>(type: "integer", nullable: false),
                    StudentNumber = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Course = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LoginCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Permissions = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teacher",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teacher", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeacherCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ActiveUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Confirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Students_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Students_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FotoUrl = table.Column<string>(type: "text", nullable: true),
                    Motto = table.Column<string>(type: "text", nullable: true),
                    Hobbys = table.Column<string>(type: "text", nullable: true),
                    Zukunftswunsch = table.Column<string>(type: "text", nullable: true),
                    IstFreigegeben = table.Column<bool>(type: "boolean", nullable: false),
                    AdminKommentar = table.Column<string>(type: "text", nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryStudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterStudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VotedStudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VotedStudent2Id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentVotes_StudentCategories_CategoryStudentId",
                        column: x => x.CategoryStudentId,
                        principalTable: "StudentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentVotes_Students_VotedStudent2Id",
                        column: x => x.VotedStudent2Id,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentVotes_Students_VotedStudentId",
                        column: x => x.VotedStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentVotes_Students_VoterStudentId",
                        column: x => x.VoterStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedByStudentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quote = table.Column<string>(type: "text", nullable: false),
                    Context = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsReleased = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherQuotes_Students_SubmittedByStudentId",
                        column: x => x.SubmittedByStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TeacherQuotes_Teacher_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teacher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryTeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterStudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VotedTeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherVotes_Students_VoterStudentId",
                        column: x => x.VoterStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherVotes_TeacherCategories_CategoryTeacherId",
                        column: x => x.CategoryTeacherId,
                        principalTable: "TeacherCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherVotes_Teacher_VotedTeacherId",
                        column: x => x.VotedTeacherId,
                        principalTable: "Teacher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Username",
                table: "AdminUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TargetId",
                table: "Comments",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCategories_QuestionGroupId",
                table: "StudentCategories",
                column: "QuestionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_StudentId",
                table: "StudentProfiles",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_LoginCode",
                table: "Students",
                column: "LoginCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentVotes_CategoryStudentId_VoterStudentId",
                table: "StudentVotes",
                columns: new[] { "CategoryStudentId", "VoterStudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentVotes_VotedStudent2Id",
                table: "StudentVotes",
                column: "VotedStudent2Id");

            migrationBuilder.CreateIndex(
                name: "IX_StudentVotes_VotedStudentId",
                table: "StudentVotes",
                column: "VotedStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentVotes_VoterStudentId",
                table: "StudentVotes",
                column: "VoterStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherCategories_QuestionGroupId",
                table: "TeacherCategories",
                column: "QuestionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherQuotes_SubmittedByStudentId",
                table: "TeacherQuotes",
                column: "SubmittedByStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherQuotes_TeacherId",
                table: "TeacherQuotes",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherVotes_CategoryTeacherId_VoterStudentId",
                table: "TeacherVotes",
                columns: new[] { "CategoryTeacherId", "VoterStudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherVotes_VotedTeacherId",
                table: "TeacherVotes",
                column: "VotedTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherVotes_VoterStudentId",
                table: "TeacherVotes",
                column: "VoterStudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminUsers");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "StudentProfiles");

            migrationBuilder.DropTable(
                name: "StudentVotes");

            migrationBuilder.DropTable(
                name: "TeacherQuotes");

            migrationBuilder.DropTable(
                name: "TeacherVotes");

            migrationBuilder.DropTable(
                name: "StudentCategories");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "TeacherCategories");

            migrationBuilder.DropTable(
                name: "Teacher");
        }
    }
}
