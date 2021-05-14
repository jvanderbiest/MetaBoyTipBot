using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
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

        public UpdateController(IUpdateService updateService, IOptions<BotConfiguration> botConfiguration)
        {
            _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));
            _botConfiguration = botConfiguration ?? throw new ArgumentNullException(nameof(botConfiguration));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update, [FromQuery] string token)
        {
            if (_botConfiguration.Value.VerifyToken != token)
            {
                return Unauthorized();
            }

            await _updateService.Update(update);
            return Ok();
        }
    }
}