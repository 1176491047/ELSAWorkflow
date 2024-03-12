using Elsa.Activities.Console;
using Elsa.Activities.Temporal;
using Elsa.Builders;
using NodaTime;
using System.IO;
using System;
using Microsoft.Extensions.Configuration;
using Elsa.Server.Api.Endpoints.Activities;
using System.Collections.Generic;
using System.Linq;
using ElsaQuickstarts.Server.DashboardAndServer.Common;

namespace ElsaQuickstarts.Server.DashboardAndServer
{
    public class ImageCleanWorkflow : IWorkflow
    {
        private  Logger aLogger;
        private IConfiguration _configuration;

        public ImageCleanWorkflow(IConfiguration configuration)
        {
            _configuration = configuration;
            aLogger = Logger.Default;
        }

        //每23小时执行一次图片删除任务
        public void Build(IWorkflowBuilder builder) =>
            builder
                .Timer(Duration.FromHours(1))
                .WriteLine(() => $"{DateTime.Now} ：开始{CelanImage()}");



        private string CelanImage()
        {
            try
            {
                List<string> deletedImages = new List<string>();
                int imageSaveDays = Convert.ToInt32(_configuration["ImageSaveDays"]);
                string folderPath = @$"{AppDomain.CurrentDomain.BaseDirectory}wwwroot\Images";
                DirectoryInfo d = new DirectoryInfo(folderPath);
                DateTime beforTime = DateTime.Now.AddDays(-imageSaveDays);
                aLogger.Info($"开始执行删除任务 保留天数:{imageSaveDays},删除{beforTime}前的文件", "图片删除任务");
                FileInfo[] files = d.GetFiles().Where(x=> x.CreationTime <= beforTime).ToArray();//文件
                foreach (var item in files)
                {
                    item.Delete();
                    aLogger.Info($"图片[{item.FullName}]删除成功 图片创建时间：[{item.CreationTime}]", "图片删除任务");
                }
                ////获取图片文件夹下的所有子文件夹
                //string[] allSubFolders = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);


                //List<string> saveDays = new List<string>();
                //for (int i = 0; i < imageSaveDays; i++)
                //{
                //    saveDays.Add(DateTime.Now.AddDays(-i).ToString("yyyy-MM-dd"));
                //}

                //foreach (string folder in allSubFolders)
                //{
                //    if (!saveDays.Contains(folder.Split('\\').Last()))
                //    {
                //        Directory.Delete(folder, true);
                //        aLogger.Info($"文件夹{folder}删除成功。", "图片删除任务");
                //        Console.WriteLine($"文件夹{folder}删除成功。");
                //    }
                //}
            }
            catch (Exception ex)
            {
                aLogger.Info($"图片删除发生异常{ex}","图片删除任务");
                Console.WriteLine("删除图片文件发生错误: " + ex.Message);
            }
            return "执行删除";
        }
    }
}
