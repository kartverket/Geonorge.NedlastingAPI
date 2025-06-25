using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Geonorge.Download.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClipperFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateUploaded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    File = table.Column<string>(type: "text", nullable: true),
                    Valid = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClipperFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dataset",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tittel = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    metadataUuid = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    supportsAreaSelection = table.Column<bool>(type: "boolean", nullable: true),
                    supportsFormatSelection = table.Column<bool>(type: "boolean", nullable: true),
                    supportsPolygonSelection = table.Column<bool>(type: "boolean", nullable: true),
                    supportsProjectionSelection = table.Column<bool>(type: "boolean", nullable: true),
                    fmeklippeUrl = table.Column<string>(type: "text", nullable: true),
                    mapSelectionLayer = table.Column<string>(type: "text", nullable: true),
                    AccessConstraint = table.Column<string>(type: "text", nullable: true),
                    AccessConstraintRequiredRole = table.Column<string>(type: "text", nullable: true),
                    maxArea = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dataset", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DownloadUsage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Uuid = table.Column<string>(type: "text", nullable: true),
                    AreaCode = table.Column<string>(type: "text", nullable: true),
                    AreaName = table.Column<string>(type: "text", nullable: true),
                    Format = table.Column<string>(type: "text", nullable: true),
                    Projection = table.Column<string>(type: "text", nullable: true),
                    Group = table.Column<string>(type: "text", nullable: true),
                    Purpose = table.Column<string>(type: "text", nullable: true),
                    SoftwareClientVersion = table.Column<string>(type: "text", nullable: true),
                    SoftwareClient = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadUsage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineAccounts",
                columns: table => new
                {
                    Username = table.Column<string>(type: "text", nullable: false),
                    Passsword = table.Column<string>(type: "text", nullable: true),
                    Company = table.Column<string>(type: "text", nullable: true),
                    ContactPerson = table.Column<string>(type: "text", nullable: true),
                    ContactEmail = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Roles = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineAccounts", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "orderDownload",
                columns: table => new
                {
                    referenceNumber = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    orderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    username = table.Column<string>(type: "text", nullable: true),
                    DownloadAsBundle = table.Column<bool>(type: "boolean", nullable: false),
                    DownloadBundleUrl = table.Column<string>(type: "text", nullable: true),
                    DownloadBundleNotificationSent = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderDownload", x => x.referenceNumber);
                });

            migrationBuilder.CreateTable(
                name: "filliste",
                columns: table => new
                {
                    filnavn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    url = table.Column<string>(type: "text", nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kategori = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    underkategori = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    inndeling = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    inndelingsverdi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    projeksjon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    dataset = table.Column<int>(type: "integer", nullable: true),
                    AccessConstraintRequiredRole = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    FileUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    DownloadUrl = table.Column<string>(type: "text", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    ReferenceNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Format = table.Column<string>(type: "text", nullable: true),
                    Area = table.Column<string>(type: "text", nullable: true),
                    AreaName = table.Column<string>(type: "text", nullable: true),
                    Coordinates = table.Column<string>(type: "text", nullable: true),
                    CoordinateSystem = table.Column<string>(type: "text", nullable: true),
                    ClipperFile = table.Column<string>(type: "text", nullable: true),
                    Projection = table.Column<string>(type: "text", nullable: true),
                    ProjectionName = table.Column<string>(type: "text", nullable: true),
                    MetadataUuid = table.Column<string>(type: "text", nullable: true),
                    MetadataName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_orderItem_orderDownload_ReferenceNumber",
                        column: x => x.ReferenceNumber,
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
                name: "IDX_OrderUuid",
                table: "orderDownload",
                column: "Uuid");

            migrationBuilder.CreateIndex(
                name: "IDX_FileUuid",
                table: "orderItem",
                column: "FileUuid");

            migrationBuilder.CreateIndex(
                name: "IDX_OrderItemUuid",
                table: "orderItem",
                column: "Uuid");

            migrationBuilder.CreateIndex(
                name: "IX_orderItem_ReferenceNumber",
                table: "orderItem",
                column: "ReferenceNumber");
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
