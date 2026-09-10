using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frontend.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentProfiles_StudentId",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "AdminKommentar",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "Hobbys",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "IstFreigegeben",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "Motto",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "Zukunftswunsch",
                table: "StudentProfiles");

            migrationBuilder.RenameColumn(
                name: "ErstelltAm",
                table: "StudentProfiles",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "AktualisiertAm",
                table: "StudentProfiles",
                newName: "UpdatedAt");

            migrationBuilder.CreateTable(
                name: "ProfileCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfileFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Placeholder = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileFields_ProfileCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProfileCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileFieldOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileFieldOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileFieldOptions_ProfileFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ProfileFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileValues_ProfileFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ProfileFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfileValues_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileValueOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileValueId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldOptionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileValueOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileValueOptions_ProfileFieldOptions_FieldOptionId",
                        column: x => x.FieldOptionId,
                        principalTable: "ProfileFieldOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfileValueOptions_ProfileValues_ProfileValueId",
                        column: x => x.ProfileValueId,
                        principalTable: "ProfileValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_StudentId",
                table: "StudentProfiles",
                column: "StudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileFieldOptions_FieldId_Value",
                table: "ProfileFieldOptions",
                columns: new[] { "FieldId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileFields_CategoryId_SortOrder",
                table: "ProfileFields",
                columns: new[] { "CategoryId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileValueOptions_FieldOptionId",
                table: "ProfileValueOptions",
                column: "FieldOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileValueOptions_ProfileValueId_FieldOptionId",
                table: "ProfileValueOptions",
                columns: new[] { "ProfileValueId", "FieldOptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileValues_FieldId",
                table: "ProfileValues",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileValues_StudentProfileId_FieldId",
                table: "ProfileValues",
                columns: new[] { "StudentProfileId", "FieldId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileValueOptions");

            migrationBuilder.DropTable(
                name: "ProfileFieldOptions");

            migrationBuilder.DropTable(
                name: "ProfileValues");

            migrationBuilder.DropTable(
                name: "ProfileFields");

            migrationBuilder.DropTable(
                name: "ProfileCategories");

            migrationBuilder.DropIndex(
                name: "IX_StudentProfiles_StudentId",
                table: "StudentProfiles");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "StudentProfiles",
                newName: "AktualisiertAm");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "StudentProfiles",
                newName: "ErstelltAm");

            migrationBuilder.AddColumn<string>(
                name: "AdminKommentar",
                table: "StudentProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "StudentProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hobbys",
                table: "StudentProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IstFreigegeben",
                table: "StudentProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Motto",
                table: "StudentProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Zukunftswunsch",
                table: "StudentProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_StudentId",
                table: "StudentProfiles",
                column: "StudentId");
        }
    }
}
