using Newtonsoft.Json;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool.Models
{
    /// <summary>
    /// 发送消息结果
    /// 错误码定义地址
    /// https://open.dingtalk.com/document/orgapp/custom-robots-send-group-messages
    /// </summary>
    public class SendResult
    {
        [JsonProperty("errmsg")]
        public string ErrMsg { get; set; }

        [JsonProperty("errcode")]
        public int ErrCode { get; set; }
    }
}
