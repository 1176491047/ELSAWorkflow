using Newtonsoft.Json;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool.Models
{
    /// <summary>
    /// Marddown 类型
    /// </summary>
    public class MarkdownMessage : DingtalkMessage
    {
        public MarkdownMessage():base("markdown")
        {    
        }
        /// <summary>
        /// 首屏会话透出的展示内容
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }
        /// <summary>
        /// markdown格式的消息
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }
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
            var temp = prefix.Substring(0, prefix.Length - 1);
            var subfix = temp + "," + At.ToJson() + "}";
            return subfix;
        }
    }
}
