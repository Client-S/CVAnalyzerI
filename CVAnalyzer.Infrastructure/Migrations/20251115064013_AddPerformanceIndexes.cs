using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVAnalyzer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentClusters_CreatedByUserId",
                table: "StudentClusters");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSkills_StudentId",
                table: "StudentSkills",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClusters_CreatedByUserId_CreatedDate",
                table: "StudentClusters",
                columns: new[] { "CreatedByUserId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentClusters_CreatedDate",
                table: "StudentClusters",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Category",
                table: "Skills",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_SkillName",
                table: "Skills",
                column: "SkillName");

            migrationBuilder.CreateIndex(
                name: "IX_CVDocuments_StudentId_UploadDate",
                table: "CVDocuments",
                columns: new[] { "StudentId", "UploadDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CVDocuments_UploadDate",
                table: "CVDocuments",
                column: "UploadDate");

            migrationBuilder.CreateIndex(
                name: "IX_ClusterMembers_ClusterId",
                table: "ClusterMembers",
                column: "ClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityId",
                table: "AuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentSkills_StudentId",
                table: "StudentSkills");

            migrationBuilder.DropIndex(
                name: "IX_StudentClusters_CreatedByUserId_CreatedDate",
                table: "StudentClusters");

            migrationBuilder.DropIndex(
                name: "IX_StudentClusters_CreatedDate",
                table: "StudentClusters");

            migrationBuilder.DropIndex(
                name: "IX_Skills_Category",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Skills_SkillName",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_CVDocuments_StudentId_UploadDate",
                table: "CVDocuments");

            migrationBuilder.DropIndex(
                name: "IX_CVDocuments_UploadDate",
                table: "CVDocuments");

            migrationBuilder.DropIndex(
                name: "IX_ClusterMembers_ClusterId",
                table: "ClusterMembers");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClusters_CreatedByUserId",
                table: "StudentClusters",
                column: "CreatedByUserId");
        }
    }
}
