using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DiziSearch.Migrations
{
    public partial class Initial2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Diziler",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Alias = table.Column<string>(nullable: true),
                    Year = table.Column<DateTime>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false),
                    Country = table.Column<string>(nullable: true),
                    Cast = table.Column<string>(nullable: true),
                    IMDBScore = table.Column<decimal>(nullable: false),
                    IMDBScoreStr = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Genre = table.Column<string>(nullable: true),
                    Approved = table.Column<bool>(nullable: false),
                    InFront = table.Column<bool>(nullable: false),
                    UploadedBy = table.Column<string>(nullable: true),
                    ApprovedBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diziler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false),
                    Season = table.Column<string>(nullable: false),
                    Ep = table.Column<string>(nullable: false),
                    Durum = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Alias = table.Column<string>(nullable: true),
                    Spoiler = table.Column<string>(nullable: true),
                    Subtitle = table.Column<string>(nullable: true),
                    Approved = table.Column<bool>(nullable: false),
                    EpName = table.Column<string>(nullable: true),
                    Link1 = table.Column<string>(nullable: true),
                    Link2 = table.Column<string>(nullable: true),
                    Link3 = table.Column<string>(nullable: true),
                    Link4 = table.Column<string>(nullable: true),
                    Link5 = table.Column<string>(nullable: true),
                    CurrentLink = table.Column<string>(nullable: true),
                    UploadedBy = table.Column<string>(nullable: true),
                    ApprovedBy = table.Column<string>(nullable: true),
                    EditedBy = table.Column<string>(nullable: true),
                    DiziId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Episodes_Diziler_DiziId",
                        column: x => x.DiziId,
                        principalTable: "Diziler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_DiziId",
                table: "Episodes",
                column: "DiziId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "Diziler");
        }
    }
}
