using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElsaQuickstarts.Server.DashboardAndServer.Common.SendWeChat
{
    public class GetTokenResult
    {
        public string errcode { get; set; }
        public string errmsg { get; set; }
        public string access_token  { get; set; }
        public string expires_in { get; set; }
    }
}
