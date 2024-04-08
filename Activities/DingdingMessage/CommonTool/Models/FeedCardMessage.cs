using Newtonsoft.Json;
using System.Collections.Generic;


namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool.Models
{
    /// <summary>
    /// FeedCard 类型
    /// </summary>
    public class FeedCardMessage : DingtalkMessage
    {
        public FeedCardMessage() : base("feedCard")
        {
        }

        /// <summary>
        /// 列表
        /// </summary>
        [JsonProperty("links")]
        public List<FeedCardItem> FeedCardItems { get; set; }


        public override string GetContent()
        {
            var baseJson = base.GetContent();
            var thisJon = JsonConvert.SerializeObject(this);
            return baseJson.Replace("@", thisJon);
        }
    }

    /// <summary>
    /// FeedCard类型Item
    /// </summary>
    public class FeedCardItem
    {
        /// <summary>
        /// 单条信息文本
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 点击单条信息到跳转链接
        /// </summary>
        [JsonProperty("messageURL")]
        public string MessageURL { get; set; }

        /// <summary>
        /// 单条信息后面图片的URL
        /// </summary>
        [JsonProperty("picURL")]
        public string PicURL { get; set; }
    }
}
