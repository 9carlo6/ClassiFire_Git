using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApp_FireSmoke.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassifiedImages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(nullable: false),
                    Latitude = table.Column<double>(nullable: false),
                    Longitude = table.Column<double>(nullable: false),
                    GeoPolygon = table.Column<string>(nullable: true),
                    Photo = table.Column<byte[]>(nullable: true),
                    PhotoName = table.Column<string>(nullable: true),
                    FileType = table.Column<string>(nullable: true),
                    Geolocalization = table.Column<string>(nullable: true),
                    GeoJSON = table.Column<string>(nullable: true),
                    FireTypeClassification = table.Column<string>(nullable: true),
                    SmokeTypeClassification = table.Column<string>(nullable: true),
                    FireScoreClassification = table.Column<double>(nullable: false),
                    SmokeScoreClassification = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassifiedImages", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassifiedImages");
        }
    }
}
