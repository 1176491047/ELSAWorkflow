using Elsa;
using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Persistence.EntityFramework.Sqlite;
using Elsa.Persistence.EntityFramework.SqlServer;
//using Elsa.Persistence.EntityFramework.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Data.Common;
using ElsaQuickstarts.Server.DashboardAndServer.Activities;
using Elsa.Persistence.EntityFramework.PostgreSql;
using Elsa.Persistence.EntityFramework.Oracle;
using ElsaQuickstarts.Server.DashboardAndServer.Activities.SendMail;
using ElsaQuickstarts.Server.DashboardAndServer.Activities.MessageHandling;
using ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat;

namespace ElsaQuickstarts.Server.DashboardAndServer
{
    public class Startup
    {
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }


        readonly string MyAllowSpecificOrigins = "*";
        private IWebHostEnvironment Environment { get; }
        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var elsaSection = Configuration.GetSection("Elsa");

           System.Console.WriteLine(elsaSection["DB"]);
            services
                .AddElsa(elsa => elsa
                     //.UseEntityFrameworkPersistence(ef => ef.UseSqlite())
                     //.UseEntityFrameworkPersistence(ef=>ef.UseMySql("Server=localhost;Port=3306;Database=elsa;User=root;Password=vsky123;"))
                     //  .UseEntityFrameworkPersistence(ef=>ef.UsePostgreSql(elsaSection["DB"]))
                     .UseEntityFrameworkPersistence(ef=>ef.UseSqlServer(elsaSection["DB"]))
                    //.UseEntityFrameworkPersistence(ef => ef.UseOracle(elsaSection["DB"]))
                    .AddConsoleActivities()
                    .AddEmailActivities(elsaSection.GetSection("Smtp").Bind)
                    //处理http请求的ACTIVE
                    .AddHttpActivities(elsaSection.GetSection("Server").Bind)
                    .AddActivitiesFrom<FileHandling>()
                    .AddActivitiesFrom<SendMail>()
                    .AddActivitiesFrom<SendMailWithSMTPConfig>()
                    .AddActivitiesFrom<WeiChatActivity>()
                    .AddActivitiesFrom<WeiChatActivityWithContentType>()
                    //注册定期调用包含基于时间的活动的工作流的托管服务。
                    .AddQuartzTemporalActivities()  
                    //.AddWorkflowsFrom<HeartbeatWorkflow>()
                    .AddWorkflowsFrom<Startup>()
                );
            services.AddSession();

            // Elsa API endpoints.
            services.AddElsaApiEndpoints();

            // For Dashboard.
            services.AddRazorPages();

            services.AddCors(options => {
                options.AddPolicy(MyAllowSpecificOrigins, builder =>
                {
                    builder.SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });
            //配置Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo() { Title = "my api", Version = "v1" });
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
                var xmlPath = Path.Combine(basePath, "ElsaQuickstarts.Server.DashboardAndServer.xml");
                c.IncludeXmlComments(xmlPath);
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCors(MyAllowSpecificOrigins);
            app.UseSession();
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //启用中间件服务生成Swagger作为JSON终结点
            app.UseSwagger();
            //启用中间件服务对swagger-ui，指定Swagger JSON终结点
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "my api");
            });

            app
                //静态文件 css、JS、图片等
                .UseStaticFiles() // For Dashboard.
                //在与 HTTP 活动一起使用时，需要以下调用来注册必要的中间件：
                .UseHttpActivities()
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    // Elsa API Endpoints are implemented as regular ASP.NET Core API controllers.
                    endpoints.MapControllers();

                    // For Dashboard.
                    endpoints.MapFallbackToPage("/_Host");
                });
        }
    }
}