using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NtFreX.Blog.Data;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public class TransactionActionFilter : IAsyncActionFilter
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly ILogger logger;

        public TransactionActionFilter(IConnectionFactory connectionFactory, ILogger<TransactionActionFilter> logger)
        {
            this.connectionFactory = connectionFactory;
            this.logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method.ToUpper();
            if (method == "GET" || method == "HEAD" || method == "CONNECT" || method == "OPTIONS" || method == "TRACE")
            {
                await next();
                return;
            }

            logger.LogInformation($"Starting transaction for method {method}");
            await connectionFactory.BeginTransactionAsync();

            try
            {
                await next();

                logger.LogInformation("Commiting transaction");
                await connectionFactory.CommitTansactionAsync();
            }
            catch
            {
                logger.LogInformation("Rolling transaction back");
                await connectionFactory.RollbackTansactionAsync();
                throw;
            }

        }
    }
}
