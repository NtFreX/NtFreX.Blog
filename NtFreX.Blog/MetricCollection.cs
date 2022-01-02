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
        private readonly TimeSpan bucketTimeSpan;

        public MetricCollection(int maxBucketCount, int bucketTimeSpanInSeconds)
            : this(maxBucketCount, TimeSpan.FromSeconds(bucketTimeSpanInSeconds)) { }
        public MetricCollection(int maxBucketCount, TimeSpan bucketTimeSpan)
        {
            this.metrics = new FixedAddOnlyCollection<Metric<T>>(maxBucketCount);
            this.bucketTimeSpan = bucketTimeSpan;
        }

        public IEnumerable<T> GetIncomplete(int dataPoints)
            => GetIncomplete(dataPoints, out _);
        public IEnumerable<T> GetIncomplete(int dataPoints, out int actualDataPoints)
        {
            var bucket = ToBucketStartTime(DateTime.UtcNow);
            var begin = bucket - bucketTimeSpan * dataPoints;
            var end = bucket + bucketTimeSpan;
            return metrics.PeekIncomplete(dataPoints, out actualDataPoints).Where(x => x.BucketStartTime <= end && x.BucketStartTime >= begin).Select(x => x.Value);
        }

        public void AddOrUpdate(Func<T> addAction, Action<T> updateAction)
        {
            lock (lockObj)
            {
                var now = DateTime.UtcNow;
                if (!metrics.TryPeek(out var metric))
                {
                    AddNewMetric(now, addAction);
                }
                else if (metric.BucketStartTime.Add(bucketTimeSpan) <= now || now < metric.BucketStartTime)
                {
                    AddNewMetric(now, addAction);
                }
                else
                {
                    updateAction(metric.Value);
                }
            }
        }

        private DateTime ToBucketStartTime(DateTime time)
            => time.AddTicks(-(time.Ticks % bucketTimeSpan.Ticks));

        private void AddNewMetric(DateTime time, Func<T> addAction)
        {
            var metric = new Metric<T>
            {
                Value = addAction(),
                BucketStartTime = ToBucketStartTime(time)
            };
            metrics.Add(metric);
        }
    }
}