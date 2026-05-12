using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelryFactory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BomTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BomTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DesignCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DesignName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OverheadPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BomLaborLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BomTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    EstimatedHours = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    HourlyRateThb = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomLaborLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BomLaborLines_BomTemplates_BomTemplateId",
                        column: x => x.BomTemplateId,
                        principalTable: "BomTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BomMaterialLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BomTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    MaterialDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Karat = table.Column<int>(type: "int", nullable: true),
                    PurityFraction = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    QuantityGrams = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    ExpectedLossPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    UnitCostThbPerGram = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomMaterialLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BomMaterialLines_BomTemplates_BomTemplateId",
                        column: x => x.BomTemplateId,
                        principalTable: "BomTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BomStoneLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BomTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoneType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StoneShape = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CaratPerStone = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TrackingType = table.Column<int>(type: "int", nullable: false),
                    UnitCostThbPerCarat = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomStoneLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BomStoneLines_BomTemplates_BomTemplateId",
                        column: x => x.BomTemplateId,
                        principalTable: "BomTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BomLaborLines_BomTemplateId",
                table: "BomLaborLines",
                column: "BomTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_BomMaterialLines_BomTemplateId",
                table: "BomMaterialLines",
                column: "BomTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_BomStoneLines_BomTemplateId",
                table: "BomStoneLines",
                column: "BomTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_BomTemplates_DesignCode",
                table: "BomTemplates",
                column: "DesignCode",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BomLaborLines");

            migrationBuilder.DropTable(
                name: "BomMaterialLines");

            migrationBuilder.DropTable(
                name: "BomStoneLines");

            migrationBuilder.DropTable(
                name: "BomTemplates");
        }
    }
}
