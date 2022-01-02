using System;
using System.Collections.Generic;
using System.Linq;

namespace NtFreX.Blog
{
    public class MetricCollection<T>
    {
        public class Metric<TMetric>
        {
            public TMetric Value { get; set; }
            public DateTime BucketStartTime { get; set; }
        }

        private readonly object lockObj = new object();
        private readonly FixedAddOnlyCollection<Metric<T>> metrics;
        private readonly TimeSpan metricsPer;

        public MetricCollection(int maxSize, int metricsPerSecond)
            : this(maxSize, TimeSpan.FromSeconds(metricsPerSecond)) { }
        public MetricCollection(int maxSize, TimeSpan metricsPer)
        {
            this.metrics = new FixedAddOnlyCollection<Metric<T>>(maxSize);
            this.metricsPer = metricsPer;
        }

        public IEnumerable<T> GetIncomplete(int dataPoints)
            => GetIncomplete(dataPoints, out _);
        public IEnumerable<T> GetIncomplete(int dataPoints, out int actualDataPoints)
            => metrics.PeekIncomplete(dataPoints, out actualDataPoints).Select(x => x.Value);

        public void AddOrUpdate(Func<T> addAction, Action<T> updateAction)
        {
            lock (lockObj)
            {
                var now = DateTime.UtcNow;
                if (!metrics.TryPeek(out var metric))
                {
                    AddNewMetric(now, addAction);
                }
                else if (metric.BucketStartTime.Add(metricsPer) <= now || now < metric.BucketStartTime)
                {
                    AddNewMetric(now, addAction);
                }
                else
                {
                    updateAction(metric.Value);
                }
            }
        }

        private void AddNewMetric(DateTime time, Func<T> addAction)
        {
            var metric = new Metric<T>
            {
                Value = addAction(),
                BucketStartTime = time.AddTicks(-(time.Ticks % metricsPer.Ticks))
            };
            metrics.Add(metric);
        }
    }
}