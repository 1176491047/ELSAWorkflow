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




    }
}
