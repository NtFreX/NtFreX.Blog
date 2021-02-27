using App.Metrics;
using App.Metrics.Filtering;
using App.Metrics.Filters;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Ascii;
using App.Metrics.Reporting;
using MongoDB.Driver;
using NtFreX.Blog.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NtFreX.Blog.Web
{
    public class DatabaseMetricsReporter : IReportMetrics
    {
        private IMongoCollection<MetricsDataValueSource> collection;

        public IFilterMetrics Filter { get; set; } = new MetricsFilter();
        public TimeSpan FlushInterval { get; set; }
        public IMetricsOutputFormatter Formatter { get; set; } = new MetricsTextOutputFormatter();

        public DatabaseMetricsReporter(Database database, TimeSpan flushInterval)
        {
            collection = database.Monitoring.GetCollection<MetricsDataValueSource>("appmetrics");

            FlushInterval = flushInterval;
        }

        public async Task<bool> FlushAsync(MetricsDataValueSource metricsData, CancellationToken cancellationToken = default)
        {
            await collection.InsertOneAsync(metricsData, new InsertOneOptions(), cancellationToken);
            return true;
        }
    }
}
