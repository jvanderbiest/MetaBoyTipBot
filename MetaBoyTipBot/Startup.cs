using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Extensions;
using MetaBoyTipBot.Jobs;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.Services.Conversation;
using Quartz;

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
            services.AddHttpClient();

            services.AddTransient<IMessageFactory, MessageFactory>();

            services.AddScoped<IUpdateService, UpdateService>();
            services.AddScoped<ITipService, TipService>();
            services.AddScoped<ITableStorageService, TableStorageService>();
            services.AddScoped<IBalanceService, BalanceService>();
            services.AddScoped<ITopUpService, TopUpService>();
            services.AddScoped<ITransactionHandlerService, TransactionHandlerService>();

            services.AddScoped<ITransactionHistoryRepository, TransactionHistoryRepository>();
            services.AddScoped<IUserBalanceRepository, UserBalanceRepository>();
            services.AddScoped<IWalletUserRepository, WalletUserRepository>();
            services.AddScoped<ITransactionCheckHistoryRepository, TransactionCheckHistoryRepository>();
            services.AddScoped<IUserBalanceHistoryRepository, UserBalanceHistoryRepository>();
            
            services.AddScoped<IMhcHttpClient, MhcHttpClient>();
            
            services.AddScoped<PrivateMessageService>()
                .AddScoped<IMessageService, PrivateMessageService>(s => s.GetService<PrivateMessageService>());
            services.AddScoped<GroupMessageService>()
                .AddScoped<IMessageService, GroupMessageService>(s => s.GetService<GroupMessageService>());
            services.AddScoped<CallbackMessageService>()
                .AddScoped<IMessageService, CallbackMessageService>(s => s.GetService<CallbackMessageService>());

            services.AddSingleton<IBotService, BotService>();

            services.AddScoped<IUserBalanceRepository, UserBalanceRepository>();

            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionScopedJobFactory();
                q.AddJobAndTrigger<TransactionSyncJob>(Configuration);
            });

            services.AddQuartzServer(options =>
            {
                options.WaitForJobsToComplete = true;
            });

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
