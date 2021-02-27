using App.Metrics;
using App.Metrics.Builder;
using NtFreX.Blog.Data;
using System;

namespace NtFreX.Blog.Web
{
    public static class MetricsDatabaseReporterBuilder
    {
        public static IMetricsBuilder ToDatabase(this IMetricsReportingBuilder reportingBuilder, Database database, TimeSpan flushInterval)
        {
            if (reportingBuilder == null)
            {
                throw new ArgumentNullException(nameof(reportingBuilder));
            }

            return reportingBuilder.Using(new DatabaseMetricsReporter(database, flushInterval));
        }

    }
}
