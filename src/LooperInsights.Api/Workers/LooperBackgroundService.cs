using System.Diagnostics;
using LooperInsights.Api.Metrics;

namespace LooperInsights.Api.Workers;

public sealed class LooperBackgroundService(
    LooperMetrics metrics,
    ILogger<LooperBackgroundService> logger) : BackgroundService
{
    private const int SectionCount = 4;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("LooperBackgroundService starting.");

        long batchNumber = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            batchNumber++;
            logger.LogInformation("Batch {BatchNumber} starting.", batchNumber);
            metrics.ActiveBatches.Inc();
            var batchTimer = Stopwatch.StartNew();

            try
            {
                for (int section = 1; section <= SectionCount; section++)
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    var sectionLabel = $"section_{section}";
                    logger.LogDebug("Batch {BatchNumber} — Section {Section} starting.", batchNumber, section);

                    metrics.ActiveSectionsByType.WithLabels(sectionLabel).Inc();
                    await SimulateSectionWorkAsync(batchNumber, section, stoppingToken);
                    metrics.ActiveSectionsByType.WithLabels(sectionLabel).Dec();

                    logger.LogDebug(
                        "Batch {BatchNumber} — Section {Section} done.",
                        batchNumber, section);
                }

                batchTimer.Stop();
                metrics.BatchesTotal.Inc();
                metrics.BatchDurationSeconds.Observe(batchTimer.Elapsed.TotalSeconds);
                logger.LogInformation(
                    "Batch {BatchNumber} completed in {ElapsedMs}ms.",
                    batchNumber, batchTimer.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Batch {BatchNumber} cancelled.", batchNumber);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Batch {BatchNumber} failed.", batchNumber);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            finally
            {
                metrics.ActiveBatches.Dec();
            }
        }

        logger.LogInformation("LooperBackgroundService stopped.");
    }

    private static async Task SimulateSectionWorkAsync(long batch, int section, CancellationToken ct)
    {
        var delay = TimeSpan.FromMilliseconds(section * 50 + Random.Shared.Next(0, 30));
        await Task.Delay(delay, ct);
    }
}