using Prometheus;

namespace LooperInsights.Api.Metrics;

public sealed class LooperMetrics
{
    public readonly Counter BatchesTotal = Prometheus.Metrics.CreateCounter(
        "looper_batches_total",
        "Total number of batches processed.");

    public readonly Histogram BatchDurationSeconds = Prometheus.Metrics.CreateHistogram(
        "looper_batch_duration_seconds",
        "Duration of a full batch in seconds.",
        new HistogramConfiguration
        {
            Buckets = Histogram.LinearBuckets(start: 0.1, width: 0.1, count: 20)
        });

    public readonly Gauge ActiveBatches = Prometheus.Metrics.CreateGauge(
        "looper_active_batches",
        "Number of batches currently being processed.");

    public readonly Gauge ActiveSectionsByType = Prometheus.Metrics.CreateGauge(
        "looper_active_sections_by_type",
        "Number of active sections by type.",
        new GaugeConfiguration { LabelNames = ["section"] });
}
