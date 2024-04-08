using Newtonsoft.Json;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool.Models
{
    /// <summary>
    /// 链接类型
    /// </summary>
    public class LinkMessage : DingtalkMessage
    {
        public LinkMessage() : base("link")
        {
        }
        /// <summary>
        /// 消息内容。如果太长只会部分展示
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }
        /// <summary>
        /// 消息标题
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }
        /// <summary>
        /// 图片URL
        /// </summary>
        [JsonProperty("picUrl")]
        public string PicUrl { get; set; }

        /// <summary>
        /// 点击消息跳转的URL
        /// </summary>
        [JsonProperty("messageUrl")]
        public string MessageUrl { get; set; }

        public override string GetContent()
        {
            var baseJson = base.GetContent();
            var thisJon = JsonConvert.SerializeObject(this);
            return baseJson.Replace("@", thisJon);
        }
    }
}
