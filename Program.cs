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


            #region ��windowsӦ�÷���
            try
            {
                // ���ú�������������
                HostFactory.Run(x =>
                {
                    x.Service<Service>(s =>                        //2
                    {
                        // ָ���������͡���������Ϊ Service
                        s.ConstructUsing(name => new Service(args));     //3

                        // ������������ִ��ʲô
                        s.WhenStarted(tc => tc.Start());              //4

                        // ������ֹͣ��ִ��ʲô
                        s.WhenStopped(tc => tc.Stop());               //5
                    });

                    // �����ñ���ϵͳ�˺�������
                    x.RunAsLocalSystem();                            //6

                    // ����������Ϣ
                    x.SetDescription("vSkysoft Elsa");        //7
                    // ������ʾ����
                    x.SetDisplayName("vskysoft Elsa");                       //8
                    // ��������
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
    /// �������
    /// </summary>
    public class Service
    {

        private string[] args;

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="vs"></param>
        public Service(string[] vs)
        {
            args = vs;
        }

        /// <summary>
        /// ��������
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
        /// ����host
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
