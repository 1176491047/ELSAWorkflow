using ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat.ByApplication;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat.Common.Model
{
    public class MPNMessage : MessageBase
    {
        public mpnews mpnews { get; set; }

        public MPNMessage(mpnews mpnews)
        {
            base.msgtype = "mpnnews";
            this.mpnews = mpnews;
        }
    }



    public class NewsMessage : MessageBase
    {
        public mpnews news { get; set; }

        public NewsMessage(mpnews news)
        {
            base.msgtype = "news";
            this.news = news;
        }
    }
}
