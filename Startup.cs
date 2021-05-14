using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.Services;

namespace MetaBoyTipBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<BotConfiguration>(Configuration.GetSection("BotConfiguration"));

            services.AddTransient<IMessageFactory, MessageFactory>();

            services.AddScoped<IUpdateService, UpdateService>();
            services.AddScoped<ITipService, TipService>();
            services.AddScoped<ITableStorageService, TableStorageService>();

            services.AddScoped<PrivateMessageService>()
                .AddScoped<IMessageService, PrivateMessageService>(s => s.GetService<PrivateMessageService>());
            services.AddScoped<GroupMessageService>()
                .AddScoped<IMessageService, GroupMessageService>(s => s.GetService<GroupMessageService>());
            services.AddScoped<CallbackMessageService>()
                .AddScoped<IMessageService, CallbackMessageService>(s => s.GetService<CallbackMessageService>());

            services.AddSingleton<IBotService, BotService>();

            services.AddScoped<IUserBalanceRepository, UserBalanceRepository>();

            services.AddControllers().AddNewtonsoftJson();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MetaBoyTipBot", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetaBoyTipBot v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
