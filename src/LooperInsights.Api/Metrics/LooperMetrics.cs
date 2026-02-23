using Prometheus;

namespace LooperInsights.Api.Metrics;

public sealed class LooperMetrics
{
    private static readonly string[] SectionLabels = ["section"];

    public readonly Counter BatchesTotal = Prometheus.Metrics.CreateCounter(
        "looper_batches_total",
        "Total number of batches processed.");

    public readonly Counter BatchesFailedTotal = Prometheus.Metrics.CreateCounter(
        "looper_batches_failed_total",
        "Total number of batches that failed.");

    public readonly Histogram BatchDurationSeconds = Prometheus.Metrics.CreateHistogram(
        "looper_batch_duration_seconds",
        "Duration of a full batch in seconds.",
        new HistogramConfiguration
        {
            Buckets = Histogram.LinearBuckets(start: 0.1, width: 0.1, count: 20)
        });

    public readonly Counter SectionsTotal = Prometheus.Metrics.CreateCounter(
        "looper_sections_total",
        "Total number of batch sections processed.",
        new CounterConfiguration { LabelNames = SectionLabels });

    public readonly Histogram SectionDurationSeconds = Prometheus.Metrics.CreateHistogram(
        "looper_section_duration_seconds",
        "Duration of each batch section in seconds.",
        new HistogramConfiguration
        {
            LabelNames = SectionLabels,
            Buckets = Histogram.LinearBuckets(start: 0.025, width: 0.025, count: 20)
        });

    public readonly Gauge ActiveBatches = Prometheus.Metrics.CreateGauge(
        "looper_active_batches",
        "Number of batches currently being processed.");
}
