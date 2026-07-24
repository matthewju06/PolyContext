using Microsoft.EntityFrameworkCore;
using PearlMetric.GatewayApi.Contracts.Api.Runs;
using PearlMetric.GatewayApi.Data;
using PearlMetric.GatewayApi.Models;

namespace PearlMetric.GatewayApi.Services;

public sealed class ScanRunService(PearlMetricDb db)
{
    public async Task<ScanRunResponse?> CreateAsync(CreateScanRunRequest request, CancellationToken cancellationToken)
    {
        var regimenExists = await db.Regimens.AnyAsync(
            regimen => regimen.Id == request.RegimenId,
            cancellationToken);

        if (!regimenExists)
        {
            return null;
        }

        var run = new ScanRun
        {
            Id = Guid.NewGuid(),
            RegimenId = request.RegimenId,
            CapturedAtUtc = EnsureUtc(request.CapturedAtUtc),
            DeviceId = request.DeviceId.Trim(),
            Status = ScanRunStatus.Pending
        };

        db.Runs.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(run, frameCount: 0, sampleCount: 0);
    }

    public async Task<ScanRunResponse?> GetByIdAsync(Guid runId, CancellationToken cancellationToken)
    {
        var run = await db.Runs
            .AsNoTracking()
            .Where(item => item.Id == runId)
            .Select(item => new
            {
                Run = item,
                FrameCount = item.Frames.Count,
                SampleCount = item.Samples.Count
            })
            .FirstOrDefaultAsync(cancellationToken);

        return run is null
            ? null
            : ToResponse(run.Run, run.FrameCount, run.SampleCount);
    }

    public async Task<ScanRunAnalysisDetailResponse?> GetAnalysisDetailAsync(
        Guid runId,
        CancellationToken cancellationToken)
    {
        var run = await db.Runs
            .AsNoTracking()
            .Include(item => item.Frames)
            .Include(item => item.Samples)
            .Include(item => item.Calibration)
            .FirstOrDefaultAsync(item => item.Id == runId, cancellationToken);

        if (run is null)
        {
            return null;
        }

        var runResponse = ToResponse(run, run.Frames.Count, run.Samples.Count);
        CalibrationProfileResponse? calibration = run.Calibration is null
            ? null
            : new CalibrationProfileResponse(
                run.Calibration.Id,
                run.Calibration.ScanRunId,
                run.Calibration.AlignmentMatrixJson,
                run.Calibration.CalibrationReferenceValue,
                run.Calibration.CalibratedAtUtc);

        var samples = run.Samples
            .OrderBy(sample => sample.SequenceIndex)
            .Select(sample => new ColorMetricSampleResponse(
                sample.Id,
                sample.ScanRunId,
                sample.SequenceIndex,
                sample.CapturedAtUtc,
                sample.L,
                sample.A,
                sample.B,
                sample.DeltaE,
                sample.ShadeGuideValue,
                sample.ConfidenceScore))
            .ToArray();

        return new ScanRunAnalysisDetailResponse(runResponse, calibration, samples);
    }

    private static DateTime EnsureUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

    private static ScanRunResponse ToResponse(ScanRun run, int frameCount, int sampleCount) =>
        new(
            run.Id,
            run.RegimenId,
            run.CapturedAtUtc,
            run.DeviceId,
            run.Status,
            run.BaselineScanRunId,
            run.DeltaEFormula,
            run.AlgorithmVersion,
            run.FailureReason,
            frameCount,
            sampleCount);
}
