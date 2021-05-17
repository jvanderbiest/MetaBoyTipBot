using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetaBoyTipBot.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace MetaBoyTipBot.Jobs
{
    [DisallowConcurrentExecution]
    public class TransactionSyncJob : IJob
    {
        private readonly ILogger<TransactionSyncJob> _logger;
        private readonly ITransactionHandlerService _transactionHandlerService;

        public TransactionSyncJob(ILogger<TransactionSyncJob> logger, ITransactionHandlerService transactionHandlerService)
        {
            _logger = logger;
            _transactionHandlerService = transactionHandlerService ?? throw new ArgumentNullException(nameof(transactionHandlerService));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Executing transaction sync job");
            await _transactionHandlerService.ImportTransactions();
            _logger.LogInformation("Finished execution of transaction sync job");
        }
    }
}
