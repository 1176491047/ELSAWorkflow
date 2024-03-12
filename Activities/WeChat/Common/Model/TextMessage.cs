

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat.ByApplication
{
    public class TextMessage : MessageBase
    {
        public Text text { get; set; }


        public TextMessage(string content)
        {
            base.msgtype = "text";
            text = new Text
            {
                content = content
            };
        }
    }

    public class Text
    {
        public string content { get; set; }
    }
}
