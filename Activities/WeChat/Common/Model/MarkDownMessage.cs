namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat.ByApplication
{
    public class MarkDownMessage:MessageBase
    {
        public MarkDownMessage(string content)
        {
            base.msgtype = "markdown";
            markdown = new Markdown()
            {
                content = content
            };
        }

        public Markdown markdown   { get; set; }


    }

    public class Markdown
    {
        public string content { get; set; }
    }
}
