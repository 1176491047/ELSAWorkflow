using Newtonsoft.Json;
using System.Collections.Generic;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool.Models
{

    public class AtSetting
    {
        public AtSetting()
        {
            AtMobiles = new List<string>();
            AtUserIds = new List<string>();
        }

        /// <summary>
        /// 被@人的手机号
        /// </summary>
        [JsonProperty("atMobiles")]
        public List<string> AtMobiles { get; set; }


        /// <summary>
        /// 被@人的用户Id
        /// </summary>
        [JsonProperty("atUserIds")]
        public List<string> AtUserIds { get; set; }

        /// <summary>
        /// @所有人时:true,否则为:false
        /// </summary>
        [JsonProperty("isAtAll")]
        public bool IsAtAll { get; set; }

        public string ToJson()
        {
            return "\"at\":" + JsonConvert.SerializeObject(this);
        }
    }
}
