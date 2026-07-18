using Microsoft.EntityFrameworkCore;
using PearlMetric.GatewayApi.Models;

namespace PearlMetric.GatewayApi.Data;

public class PearlMetricDb : DbContext
{
    public PearlMetricDb(DbContextOptions<PearlMetricDb> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<WhiteningRegimen> Regimens => Set<WhiteningRegimen>();
    public DbSet<ScanRun> Runs => Set<ScanRun>();
    public DbSet<ColorMetricSample> ColorMetricSamples => Set<ColorMetricSample>();
    public DbSet<CalibrationProfile> CalibrationProfiles => Set<CalibrationProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ColorMetricSample>()
            .HasIndex(sample => new { sample.ScanRunId, sample.SequenceIndex });

        modelBuilder.Entity<CalibrationProfile>(entity =>
        {
            entity.HasIndex(profile => profile.ScanRunId)
                .IsUnique();

            entity.Property(profile => profile.AlignmentMatrixJson)
                .HasColumnType("jsonb");
        });
    }
}
