namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat.ByApplication
{
    public class ImageMessage : MessageBase
    {
        public Image image{ get; set; }


        public ImageMessage(string midiaId)
        {
            base.msgtype = "image";
            image = new Image() { 
            media_id = midiaId,
            };
        }
    }

    public class Image
    {
        public string media_id { get; set; }
    }
}
