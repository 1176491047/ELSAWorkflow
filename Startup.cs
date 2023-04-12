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
                    //����http�����ACTIVE
                    .AddHttpActivities(elsaSection.GetSection("Server").Bind)
                    .AddActivitiesFrom<FileHandling>()
                    .AddActivitiesFrom<SendMail>()
                    .AddActivitiesFrom<SendMailWithSMTPConfig>()
                    .AddActivitiesFrom<WeiChatActivity>()
                    .AddActivitiesFrom<WeiChatActivityWithContentType>()
                    //ע�ᶨ�ڵ��ð�������ʱ��Ļ�Ĺ��������йܷ���
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
            //����Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo() { Title = "my api", Version = "v1" });
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//��ȡӦ�ó�������Ŀ¼�����ԣ����ܹ���Ŀ¼Ӱ�죬������ô˷�����ȡ·����
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

            //�����м����������Swagger��ΪJSON�ս��
            app.UseSwagger();
            //�����м�������swagger-ui��ָ��Swagger JSON�ս��
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "my api");
            });

            app
                //��̬�ļ� css��JS��ͼƬ��
                .UseStaticFiles() // For Dashboard.
                //���� HTTP �һ��ʹ��ʱ����Ҫ���µ�����ע���Ҫ���м����
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