using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelryFactory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class QualityControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QcInspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionType = table.Column<int>(type: "int", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkOrderStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RawMaterialItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InspectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InspectorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Result = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReworkStage = table.Column<int>(type: "int", nullable: true),
                    ActualWeightGrams = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    ExpectedWeightGrams = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QcInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QcInspections_RawMaterialItems_RawMaterialItemId",
                        column: x => x.RawMaterialItemId,
                        principalTable: "RawMaterialItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QcInspections_WorkOrderStages_WorkOrderStageId",
                        column: x => x.WorkOrderStageId,
                        principalTable: "WorkOrderStages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QcInspections_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QcInspections_Workers_InspectorId",
                        column: x => x.InspectorId,
                        principalTable: "Workers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QcDefects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QcInspectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefectType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QcDefects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QcDefects_QcInspections_QcInspectionId",
                        column: x => x.QcInspectionId,
                        principalTable: "QcInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QcDefects_QcInspectionId",
                table: "QcDefects",
                column: "QcInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_QcInspections_InspectionDate",
                table: "QcInspections",
                column: "InspectionDate");

            migrationBuilder.CreateIndex(
                name: "IX_QcInspections_InspectorId",
                table: "QcInspections",
                column: "InspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_QcInspections_RawMaterialItemId",
                table: "QcInspections",
                column: "RawMaterialItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QcInspections_WorkOrderId_InspectionType",
                table: "QcInspections",
                columns: new[] { "WorkOrderId", "InspectionType" });

            migrationBuilder.CreateIndex(
                name: "IX_QcInspections_WorkOrderStageId",
                table: "QcInspections",
                column: "WorkOrderStageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QcDefects");

            migrationBuilder.DropTable(
                name: "QcInspections");
        }
    }
}
