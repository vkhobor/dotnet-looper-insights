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
            var itemCount = Random.Shared.Next(1, 101); // 1-100 items per batch (same for all sections)
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
                    var sectionTimer = Stopwatch.StartNew();
                    await SimulateSectionWorkAsync(batchNumber, section, itemCount, stoppingToken);
                    sectionTimer.Stop();
                    metrics.ActiveSectionsByType.WithLabels(sectionLabel).Dec();
                    metrics.SectionDurationSeconds.WithLabels(sectionLabel).Observe(sectionTimer.Elapsed.TotalSeconds);

                    logger.LogDebug(
                        "Batch {BatchNumber} — Section {Section} done.",
                        batchNumber, section);
                }

                batchTimer.Stop();
                metrics.BatchesTotal.Inc();
                metrics.BatchDurationSeconds.Observe(batchTimer.Elapsed.TotalSeconds);
                metrics.ItemsProcessedTotal.Inc(itemCount);
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

            // Simulate longer wait between batches sometimes (no work available)
            if (Random.Shared.NextDouble() < 0.3) // 30% chance of longer wait
            {
                var noWorkDelay = TimeSpan.FromMilliseconds(Random.Shared.Next(500, 2000));
                logger.LogDebug("No work available, waiting {DelayMs}ms before next batch.", noWorkDelay.TotalMilliseconds);
                await Task.Delay(noWorkDelay, stoppingToken);
            }
        }

        logger.LogInformation("LooperBackgroundService stopped.");
    }

    private async Task SimulateSectionWorkAsync(long batch, int section, int itemCount, CancellationToken ct)
    {
        for (int item = 1; item <= itemCount; item++)
        {
            ct.ThrowIfCancellationRequested();

            // Simulate processing each work item
            var itemDelay = TimeSpan.FromMilliseconds(section * 20 + Random.Shared.Next(10, 50));
            await Task.Delay(itemDelay, ct);
        }
    }
}
