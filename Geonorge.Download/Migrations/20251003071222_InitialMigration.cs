using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Geonorge.Download.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClipperFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateUploaded = table.Column<DateTime>(type: "datetime", nullable: false),
                    File = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Valid = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClipperFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dataset",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tittel = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    metadataUuid = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    supportsAreaSelection = table.Column<bool>(type: "bit", nullable: true),
                    supportsFormatSelection = table.Column<bool>(type: "bit", nullable: true),
                    supportsPolygonSelection = table.Column<bool>(type: "bit", nullable: true),
                    supportsProjectionSelection = table.Column<bool>(type: "bit", nullable: true),
                    fmeklippeUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    mapSelectionLayer = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AccessConstraint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessConstraintRequiredRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    maxArea = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dataset", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DownloadUsage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)"),
                    Timestamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    Uuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreaCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreaName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Format = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Projection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Group = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoftwareClientVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoftwareClient = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadUsage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineAccounts",
                columns: table => new
                {
                    Username = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Passsword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Roles = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineAccounts", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "orderDownload",
                columns: table => new
                {
                    referenceNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1000, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)"),
                    email = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    orderDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "getdate()"),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DownloadAsBundle = table.Column<bool>(type: "bit", nullable: false),
                    DownloadBundleUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DownloadBundleNotificationSent = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderDownload", x => x.referenceNumber);
                });

            migrationBuilder.CreateTable(
                name: "filliste",
                columns: table => new
                {
                    filnavn = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    url = table.Column<string>(type: "ntext", nullable: false),
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    kategori = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    underkategori = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    inndeling = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    inndelingsverdi = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    projeksjon = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    dataset = table.Column<int>(type: "int", nullable: true),
                    AccessConstraintRequiredRole = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filliste", x => x.id);
                    table.ForeignKey(
                        name: "FK_filliste_Dataset_dataset",
                        column: x => x.dataset,
                        principalTable: "Dataset",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "orderItem",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)"),
                    FileUuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    downloadUrl = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    fileName = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    referenceNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Format = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreaName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Coordinates = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoordinateSystem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClipperFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Projection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetadataUuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetadataName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderItem", x => x.id);
                    table.ForeignKey(
                        name: "FK_orderItem_orderDownload_referenceNumber",
                        column: x => x.referenceNumber,
                        principalTable: "orderDownload",
                        principalColumn: "referenceNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dataset_metadataUuid",
                table: "Dataset",
                column: "metadataUuid");

            migrationBuilder.CreateIndex(
                name: "IX_filliste_dataset",
                table: "filliste",
                column: "dataset");

            migrationBuilder.CreateIndex(
                name: "IX_filliste_format",
                table: "filliste",
                column: "format");

            migrationBuilder.CreateIndex(
                name: "IX_filliste_inndeling",
                table: "filliste",
                column: "inndeling");

            migrationBuilder.CreateIndex(
                name: "IX_filliste_inndelingsverdi",
                table: "filliste",
                column: "inndelingsverdi");

            migrationBuilder.CreateIndex(
                name: "IX_filliste_projeksjon",
                table: "filliste",
                column: "projeksjon");

            migrationBuilder.CreateIndex(
                name: "IDX_Uuid",
                table: "orderDownload",
                column: "Uuid");

            migrationBuilder.CreateIndex(
                name: "IDX_FileUuid",
                table: "orderItem",
                column: "FileUuid");

            migrationBuilder.CreateIndex(
                name: "IDX_Uuid",
                table: "orderItem",
                column: "Uuid");

            migrationBuilder.CreateIndex(
                name: "IX_orderItem_referenceNumber",
                table: "orderItem",
                column: "referenceNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClipperFiles");

            migrationBuilder.DropTable(
                name: "DownloadUsage");

            migrationBuilder.DropTable(
                name: "filliste");

            migrationBuilder.DropTable(
                name: "MachineAccounts");

            migrationBuilder.DropTable(
                name: "orderItem");

            migrationBuilder.DropTable(
                name: "Dataset");

            migrationBuilder.DropTable(
                name: "orderDownload");
        }
    }
}
