namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat.Common.Model
{
    public class MessageSendResult
    {
        public int errcode { get; set; }

        public string errmsg { get; set; }
        public string invaliduser { get; set; }
        public string invalidparty { get; set; }
        public string invalidtag { get; set; }
        public string unlicenseduser { get; set; }
        public string msgid { get; set; }
        public string response_code { get; set; }
    }
}
