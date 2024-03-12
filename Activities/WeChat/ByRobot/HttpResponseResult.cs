using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat
{
    public class HttpResponseResult
    {
        public string Msg { get; set; }
        public long Code { get; set; }
        public bool Success { get; set; }
        public object Data { get; set; }
    }
}
