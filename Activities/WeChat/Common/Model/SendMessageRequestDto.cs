using System.Collections.Generic;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat.ByApplication
{
    public class SendMessageRequestDto
    {

        public string ServerUrl { get; set; }

        /// <summary>
        /// 企业ID
        /// </summary>
        public string Corpid { get; set; }


        /// <summary>
        /// 企业密钥
        /// </summary>
        public string Corpsecret { get; set; }

        /// <summary>
        /// 获取token地址
        /// </summary>
        public string GetAccessTokenUrl { get; set; }

        /// <summary>
        /// 文件上传地址
        /// </summary>
        public string FileUploadUrl { get; set; }

        /// <summary>
        /// 消息发送地址
        /// </summary>
        public string MessageSendURI { get; set; }

        /// <summary>
        /// 代理ID
        /// </summary>
        public string Agentid { get; set; }

        /// <summary>
        /// 收件人
        /// </summary>
        public List<string> Receivers { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public WeiXinMsgType MessageType { get; set; }

        /// <summary>
        /// 图片文件名称
        /// </summary>
        public string ImageFileName { get; set; }

        /// <summary>
        /// 消息体json
        /// </summary>
        public string MessageContent { get; set; }


        /// <summary>
        /// 图文消息的title
        /// </summary>
        public string NewsTitle { get; set; }
    }
}
