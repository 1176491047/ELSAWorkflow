using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ElsaQuickstarts.Server.DashboardAndServer.Common
{
    public static class common
    {
        private static Logger aLogger = Logger.Default;

        public static string TestFilePath { get; set; }
        public static string ReadFirlToBASE64(string path) {
            using (FileStream filestream = new FileStream(path, FileMode.Open))
            {
                byte[] bt = new byte[filestream.Length];

                //调用read读取方法
                filestream.Read(bt, 0, bt.Length);
                var base64Str = Convert.ToBase64String(bt);

                return base64Str;
            }
        }



        public static byte[] ReadFirlToBYTE(string path)
        {


            using (FileStream filestream = new FileStream(path, FileMode.Open))
            {
                byte[] bt = new byte[filestream.Length];

                //调用read读取方法
                filestream.Read(bt, 0, bt.Length);

                return bt;
            }

        }


        public static string SaveImage(string base64String, string ImageName)
        {

            try
            { // 解码Base64数据为字节数组
                byte[] imageBytes = Convert.FromBase64String(base64String);
                string dateCode = DateTime.Now.ToShortDateString();
                //相对路径
                string parentPath = @$"{AppDomain.CurrentDomain.BaseDirectory}wwwroot\Images";
                if (!Directory.Exists(parentPath))
                {
                    Directory.CreateDirectory(parentPath);
                }
                string partPath = @$"{parentPath}\{ImageName}"; // 保存的路径和文件名
                // 创建文件流并写入字节数组
                using (FileStream fs = new FileStream(@$"{partPath}", FileMode.Create))
                {
                    fs.Write(imageBytes, 0, imageBytes.Length);
                }

                Console.WriteLine("图片已成功保存到本地文件夹。");
                return partPath;
            }
            catch (Exception ex)
            {

                aLogger.Info($"保存图片发生异常:{ex}", "企业微信异常");

                return "";
            }


        }


    }
}
