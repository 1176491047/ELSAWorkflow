using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Elsa.Activities.Primitives;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topshelf;

namespace ElsaQuickstarts.Server.DashboardAndServer
{
    public class Program
    {
        public static void Main(string[] args)
        {


            #region 以windows应用服务
            try
            {
                // 配置和运行宿主服务
                HostFactory.Run(x =>
                {
                    x.Service<Service>(s =>                        //2
                    {
                        // 指定服务类型。这里设置为 Service
                        s.ConstructUsing(name => new Service(args));     //3

                        // 当服务启动后执行什么
                        s.WhenStarted(tc => tc.Start());              //4

                        // 当服务停止后执行什么
                        s.WhenStopped(tc => tc.Stop());               //5
                    });

                    // 服务用本地系统账号来运行
                    x.RunAsLocalSystem();                            //6

                    // 服务描述信息
                    x.SetDescription("vSkysoft Elsa");        //7
                    // 服务显示名称
                    x.SetDisplayName("vskysoft Elsa");                       //8
                    // 服务名称
                    x.SetServiceName("vskysoft Elsa");                       //9 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            #endregion
        }
    }


    /// <summary>
    /// 服务入口
    /// </summary>
    public class Service
    {

        private string[] args;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="vs"></param>
        public Service(string[] vs)
        {
            args = vs;
        }

        /// <summary>
        /// 开启服务
        /// </summary>
        public void Start()
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            //XDIYThreadpool.SetThreadPool(20, 50);
            var builder = CreateWebHostBuilder(args.Where(arg => arg != "--console").ToArray());

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                builder.UseContentRoot(pathToContentRoot);
            }

            var host = builder.Build();
            host.RunAsync();
        }

        public void Stop()
        {
        }

        /// <summary>
        /// 创建host
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
            return WebHost.CreateDefaultBuilder(args)
                    .UseKestrel()
                    .UseUrls(config["Url"])
                    .UseConfiguration(config)
                    .UseStartup<Startup>();
        }
    }
}
