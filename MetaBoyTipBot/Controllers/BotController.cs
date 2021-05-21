using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LazyCache;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Extensions;
using MetaBoyTipBot.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace MetaBoyTipBot.Controllers
{
    [Route("api/updates")]
    public class UpdateController : Controller
    {
        private readonly IUpdateService _updateService;
        private readonly IOptions<BotConfiguration> _botConfiguration;
        private readonly IAppCache _cache;
        private readonly IStartupOptions _startupOptions;

        public UpdateController(IUpdateService updateService, IOptions<BotConfiguration> botConfiguration, IAppCache cache, IStartupOptions startupOptions)
        {
            _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));
            _botConfiguration = botConfiguration ?? throw new ArgumentNullException(nameof(botConfiguration));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _startupOptions = startupOptions ?? throw new ArgumentNullException(nameof(startupOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update, [FromQuery] string token)
        {
            var isOldMessage = update.Message?.Date < _startupOptions.StartDateTime;
            if (isOldMessage)
            {
                return Ok();
            }

            var existingUpdateId = _cache.Get<bool>(update.Id.ToString());
            if (existingUpdateId)
            {
                return Ok();
            }

            _cache.Add(update.Id.ToString(), true, DateTimeOffset.UtcNow.AddMinutes(5));

            await _updateService.Update(update);
            return Ok();
        }
    }
}