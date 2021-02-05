using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace sol_Job_Bank.Data.JBMigrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "JB");

            migrationBuilder.CreateTable(
                name: "Occupations",
                schema: "JB",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    Title = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Occupations", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RetrainingPrograms",
                schema: "JB",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(maxLength: 70, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetrainingPrograms", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                schema: "JB",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                schema: "JB",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Description = table.Column<string>(maxLength: 511, nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    OccupationID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Positions_Occupations_OccupationID",
                        column: x => x.OccupationID,
                        principalSchema: "JB",
                        principalTable: "Occupations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Applicants",
                schema: "JB",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    FirstName = table.Column<string>(maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(maxLength: 30, nullable: true),
                    LastName = table.Column<string>(maxLength: 50, nullable: false),
                    SIN = table.Column<string>(maxLength: 9, nullable: false),
                    Phone = table.Column<long>(nullable: false),
                    eMail = table.Column<string>(maxLength: 255, nullable: false),
                    RetrainingProgramID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicants", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Applicants_RetrainingPrograms_RetrainingProgramID",
                        column: x => x.RetrainingProgramID,
                        principalSchema: "JB",
                        principalTable: "RetrainingPrograms",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetApplicantTimestampOnUpdate
                    AFTER UPDATE ON Applicants
                    BEGIN
                        UPDATE Applicants
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                   @"
                        CREATE TRIGGER SetApplicantTimestampOnInsert
                        AFTER INSERT ON Applicants
                        BEGIN
                            UPDATE Applicants
                            SET RowVersion = randomblob(8)
                            WHERE rowid = NEW.rowid;
                        END
                  ");

            migrationBuilder.CreateTable(
                name: "PositionSkills",
                schema: "JB",
                columns: table => new
                {
                    SkillID = table.Column<int>(nullable: false),
                    PositionID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionSkills", x => new { x.PositionID, x.SkillID });
                    table.ForeignKey(
                        name: "FK_PositionSkills_Positions_PositionID",
                        column: x => x.PositionID,
                        principalSchema: "JB",
                        principalTable: "Positions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PositionSkills_Skills_SkillID",
                        column: x => x.SkillID,
                        principalSchema: "JB",
                        principalTable: "Skills",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Postings",
                schema: "JB",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    NumberOpen = table.Column<int>(nullable: false),
                    ClosingDate = table.Column<DateTime>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: true),
                    PositionID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Postings_Positions_PositionID",
                        column: x => x.PositionID,
                        principalSchema: "JB",
                        principalTable: "Positions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantSkills",
                schema: "JB",
                columns: table => new
                {
                    SkillID = table.Column<int>(nullable: false),
                    ApplicantID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantSkills", x => new { x.ApplicantID, x.SkillID });
                    table.ForeignKey(
                        name: "FK_ApplicantSkills_Applicants_ApplicantID",
                        column: x => x.ApplicantID,
                        principalSchema: "JB",
                        principalTable: "Applicants",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicantSkills_Skills_SkillID",
                        column: x => x.SkillID,
                        principalSchema: "JB",
                        principalTable: "Skills",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                schema: "JB",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    Comments = table.Column<string>(maxLength: 2000, nullable: false),
                    PostingID = table.Column<int>(nullable: false),
                    ApplicantID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Applications_Applicants_ApplicantID",
                        column: x => x.ApplicantID,
                        principalSchema: "JB",
                        principalTable: "Applicants",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Applications_Postings_PostingID",
                        column: x => x.PostingID,
                        principalSchema: "JB",
                        principalTable: "Postings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UploadedFile",
                schema: "JB",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MimeType = table.Column<string>(maxLength: 255, nullable: true),
                    FileName = table.Column<string>(maxLength: 255, nullable: true),
                    Description = table.Column<string>(maxLength: 2000, nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    ApplicantID = table.Column<int>(nullable: true),
                    ApplicantPhoto_ApplicantID = table.Column<int>(nullable: true),
                    PostingID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFile", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UploadedFile_Applicants_ApplicantID",
                        column: x => x.ApplicantID,
                        principalSchema: "JB",
                        principalTable: "Applicants",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UploadedFile_Applicants_ApplicantPhoto_ApplicantID",
                        column: x => x.ApplicantPhoto_ApplicantID,
                        principalSchema: "JB",
                        principalTable: "Applicants",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UploadedFile_Postings_PostingID",
                        column: x => x.PostingID,
                        principalSchema: "JB",
                        principalTable: "Postings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileContent",
                schema: "JB",
                columns: table => new
                {
                    FileContentID = table.Column<int>(nullable: false),
                    Content = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileContent", x => x.FileContentID);
                    table.ForeignKey(
                        name: "FK_FileContent_UploadedFile_FileContentID",
                        column: x => x.FileContentID,
                        principalSchema: "JB",
                        principalTable: "UploadedFile",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_RetrainingProgramID",
                schema: "JB",
                table: "Applicants",
                column: "RetrainingProgramID");

            migrationBuilder.CreateIndex(
                name: "IX_Unique_Applicant_email",
                schema: "JB",
                table: "Applicants",
                column: "eMail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantSkills_SkillID",
                schema: "JB",
                table: "ApplicantSkills",
                column: "SkillID");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_PostingID",
                schema: "JB",
                table: "Applications",
                column: "PostingID");

            migrationBuilder.CreateIndex(
                name: "IX_Unique_Application",
                schema: "JB",
                table: "Applications",
                columns: new[] { "ApplicantID", "PostingID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Name",
                schema: "JB",
                table: "Positions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_OccupationID",
                schema: "JB",
                table: "Positions",
                column: "OccupationID");

            migrationBuilder.CreateIndex(
                name: "IX_PositionSkills_SkillID",
                schema: "JB",
                table: "PositionSkills",
                column: "SkillID");

            migrationBuilder.CreateIndex(
                name: "IX_Postings_PositionID_ClosingDate",
                schema: "JB",
                table: "Postings",
                columns: new[] { "PositionID", "ClosingDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Name",
                schema: "JB",
                table: "Skills",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFile_ApplicantID",
                schema: "JB",
                table: "UploadedFile",
                column: "ApplicantID");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFile_ApplicantPhoto_ApplicantID",
                schema: "JB",
                table: "UploadedFile",
                column: "ApplicantPhoto_ApplicantID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFile_PostingID",
                schema: "JB",
                table: "UploadedFile",
                column: "PostingID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicantSkills",
                schema: "JB");

            migrationBuilder.DropTable(
                name: "Applications",
                schema: "JB");

            migrationBuilder.DropTable(
                name: "FileContent",
                schema: "JB");

            migrationBuilder.DropTable(
                name: "PositionSkills",
                schema: "JB");

            migrationBuilder.DropTable(
                name: "UploadedFile",
                schema: "JB");

            migrationBuilder.DropTable(
                name: "Skills",
                schema: "JB");

            migrationBuilder.DropTable(
                name: "Applicants",
                schema: "JB");

            migrationBuilder.DropTable(
                name: "Postings",
                schema: "JB");

            migrationBuilder.DropTable(
                name: "RetrainingPrograms",
                schema: "JB");

            migrationBuilder.DropTable(
                name: "Positions",
                schema: "JB");

            migrationBuilder.DropTable(
                name: "Occupations",
                schema: "JB");
        }
    }
}
