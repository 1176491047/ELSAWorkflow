using Newtonsoft.Json;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool.Models
{
    /// <summary>
    /// 文本类型
    /// </summary>
    public class TextMessage : DingtalkMessage
    {
        public TextMessage() : base("text")
        {
        }
        /// <summary>
        /// 消息内容
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// 被@的信息
        /// </summary>
        [JsonProperty("at")]
        [JsonIgnore]
        public AtSetting At { get; set; }

        public override string GetContent()
        {
            var baseJson = base.GetContent();
            var thisJon = JsonConvert.SerializeObject(this);
            var prefix = baseJson.Replace("@", thisJon);
            if (At == null)
            {
                return prefix;
            }
            var temp = prefix.Substring(0, prefix.Length - 1);
            var subfix = temp + "," + At.ToJson() + "}";
            return subfix;
        }
    }
}
