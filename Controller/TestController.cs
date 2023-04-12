using Elsa.Models;
using ElsaQuickstarts.Server.DashboardAndServer.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Elsa.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elsa.Activities.Signaling.Services;
using System.Configuration;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using ElsaQuickstarts.Server.DashboardAndServer.Common.SendWeChat;
using Microsoft.Extensions.Configuration;

namespace ElsaQuickstarts.Server.DashboardAndServer.新文件夹
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TestController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("/BASE64")]
        public string BASE64([FromBody] requestInfo request)
        {

           string result = Common.common.ReadFirlToBASE64(Common.common.TestFilePath);
            return result;
        }




        [HttpPost("/Byte")]
        public byte[] test_Byte([FromBody] requestInfo request)
        {

            byte[] result = Common.common.ReadFirlToBYTE(Common.common.TestFilePath);
            return result;
        }




        [HttpGet]
        public void SendEmial() {

            string touser = "Kiaka";
            QYWeixinHelper qYWeixinHelper = new QYWeixinHelper(_configuration, _httpContextAccessor);
            qYWeixinHelper.SendText("ZhangJiaQi","123");
        }


}

public class testbody {
        public string name  { get; set; }
    }
    public class requestInfo
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }

    }
}
