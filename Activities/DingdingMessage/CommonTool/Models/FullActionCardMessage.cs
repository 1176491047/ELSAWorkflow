using Newtonsoft.Json;


namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool.Models
{
    /// <summary>
    /// 整体跳转 ActionCard 类型
    /// </summary>
    public class FullActionCardMessage : DingtalkMessage
    {
        public FullActionCardMessage() : base("actionCard")
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
        /// 单个按钮的标题
        /// </summary>
        [JsonProperty("singleTitle")]
        public string SingleTitle { get; set; }
        /// <summary>
        /// 点击消息跳转的URL
        /// </summary>
        [JsonProperty("singleURL")]
        public string SingleURL { get; set; }
        /// <summary>
        /// "0"-按钮竖直排列，"1"-按钮横向排列
        /// </summary>
        [JsonProperty("btnOrientation")]
        public string BtnOrientation { get; set; }
        public override string GetContent()
        {
            var baseJson = base.GetContent();
            var thisJon = JsonConvert.SerializeObject(this);
            return baseJson.Replace("@", thisJon);
        }
    }
}
