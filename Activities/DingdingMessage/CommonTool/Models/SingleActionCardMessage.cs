using Newtonsoft.Json;
using System.Collections.Generic;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool.Models
{
    /// <summary>
    /// 独立跳转 ActionCard 类型
    /// </summary>
    public class SingleActionCardMessage : DingtalkMessage
    {
        public SingleActionCardMessage():base("actionCard")
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
        /// "0":按钮竖直排列，"1":按钮横向排列
        /// </summary>
        [JsonIgnore]
        [JsonProperty("btnOrientation")]
        public string BtnOrientation { get; set; }
        /// <summary>
        /// 按钮的信息 
        /// </summary>
        [JsonProperty("btns")]
        public List<SingleActionCardButton> SingleActionCardButtons { get; set; }

        public override string GetContent()
        {
            var baseJson = base.GetContent();
            var thisJon = JsonConvert.SerializeObject(this);
            return baseJson.Replace("@", thisJon);
        }
    }

    /// <summary>
    /// 按钮的信息
    /// </summary>
    public class SingleActionCardButton
    {
        /// <summary>
        /// 按钮文案
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 点击按钮触发的URL
        /// </summary>
        [JsonProperty("actionURL")]
        public string ActionURL { get; set; }
    }
}
