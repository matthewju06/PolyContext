using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GatewayApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regimens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    ScheduledIntervalHours = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regimens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regimens_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegimenId = table.Column<Guid>(type: "uuid", nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeviceId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Runs_Regimens_RegimenId",
                        column: x => x.RegimenId,
                        principalTable: "Regimens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalibrationProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScanRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlignmentMatrixJson = table.Column<string>(type: "jsonb", nullable: false),
                    CalibrationReferenceValue = table.Column<double>(type: "double precision", nullable: false),
                    CalibratedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalibrationProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalibrationProfiles_Runs_ScanRunId",
                        column: x => x.ScanRunId,
                        principalTable: "Runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ColorMetricSamples",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScanRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceIndex = table.Column<int>(type: "integer", nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    L = table.Column<double>(type: "double precision", nullable: false),
                    A = table.Column<double>(type: "double precision", nullable: false),
                    B = table.Column<double>(type: "double precision", nullable: false),
                    DeltaE = table.Column<double>(type: "double precision", nullable: false),
                    ShadeGuideValue = table.Column<string>(type: "text", nullable: true),
                    ConfidenceScore = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorMetricSamples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ColorMetricSamples_Runs_ScanRunId",
                        column: x => x.ScanRunId,
                        principalTable: "Runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationProfiles_ScanRunId",
                table: "CalibrationProfiles",
                column: "ScanRunId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ColorMetricSamples_ScanRunId_SequenceIndex",
                table: "ColorMetricSamples",
                columns: new[] { "ScanRunId", "SequenceIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_Regimens_PatientId",
                table: "Regimens",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Runs_RegimenId",
                table: "Runs",
                column: "RegimenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalibrationProfiles");

            migrationBuilder.DropTable(
                name: "ColorMetricSamples");

            migrationBuilder.DropTable(
                name: "Runs");

            migrationBuilder.DropTable(
                name: "Regimens");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
