using System;
using System.Linq;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace MetaBoyTipBot.Middleware
{
    public class TelegramAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<BotConfiguration> _botConfiguration;

        public TelegramAuthMiddleware(RequestDelegate next, IOptions<BotConfiguration> botConfiguration)
        {
            _next = next;
            _botConfiguration = botConfiguration ?? throw new ArgumentNullException(nameof(botConfiguration));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.StartsWithSegments("/api/updates"))
            {
                var token = httpContext.Request.Query["token"].FirstOrDefault();
                if (string.IsNullOrEmpty(token) || _botConfiguration.Value.VerifyToken != token)
                {
                    httpContext.Response.StatusCode = 401;
                    await httpContext.Response.WriteAsync("Unauthorized");
                    return;
                }

                await _next(httpContext);
            }

            // ignore all other requests
        }
    }
}