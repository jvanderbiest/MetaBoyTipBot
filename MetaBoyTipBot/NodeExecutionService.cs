using System;
using System.Globalization;
using System.Threading.Tasks;
using Jering.Javascript.NodeJS;
using MetaBoyTipBot.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MetaBoyTipBot
{
    public interface INodeExecutionService
    {
        Task<string> Withdraw(string toAddress, double amount);
    }

    public class Result
    {
        public string TransactionId { get; set; }
        public string Json { get; set; }
    }

    public class NodeExecutionService : INodeExecutionService
    {
        private readonly INodeJSService _nodeJsService;
        private readonly ILogger<INodeExecutionService> _logger;
        private readonly IOptions<BotConfiguration> _botConfiguration;
        private const string MainNodeFilePath = "./metahash-js/node.js";

        public NodeExecutionService(INodeJSService nodeJsService, ILogger<INodeExecutionService> logger, IOptions<BotConfiguration> botConfiguration)
        {
            _nodeJsService = nodeJsService ?? throw new ArgumentNullException(nameof(nodeJsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _botConfiguration = botConfiguration ?? throw new ArgumentNullException(nameof(botConfiguration));
        }

        /// <summary>
        /// Withdraws using the metahash-js library on a NodeJS process.
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="amount"></param>
        /// <returns>The transaction Id if succeeded, null if not</returns>
        public async Task<string> Withdraw(string toAddress, double amount)
        {
            var privateKey = _botConfiguration.Value.PrivateKey;
            var mhcHashAmount = (amount * 1000000).ToString(CultureInfo.InvariantCulture);
            var text = "MetaBoyTipBot withdrawal";
            Result result = await _nodeJsService.InvokeFromFileAsync<Result>(MainNodeFilePath, "sendTx", new object[] { privateKey, toAddress, mhcHashAmount, text });
            _logger.LogInformation($"Withdraw to {toAddress} for amount {amount} ({mhcHashAmount} hash): ", result.Json);
            return result.TransactionId;
        }
    }
}
