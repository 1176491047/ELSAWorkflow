
namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool.Models
{

    /// <summary>
    /// 抽象的消息类型
    /// </summary>
    public abstract class DingtalkMessage : IDingtalkMessage
    {
        public DingtalkMessage(string msgType)
        {
            MsgType = msgType;
        }
        /// <summary>
        /// 消息类型
        /// </summary>
        /// <returns></returns>
        public string MsgType { get; set; }
        /// <summary>
        /// 消息内容，Json格式
        /// </summary>
        /// <returns></returns>
        public virtual string GetContent()
        {
            return "{\"msgtype\": \"" + MsgType + "\",\"" + MsgType + "\" : @ }";
        }
    }
}
