using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat
{
    public class WeChatRequestDto
    {
        public string title { get; set; }
        public string content { get; set; }
        public string messageUrl { get; set; }
        public string pictureUrl { get; set; }

        public List<string> receivers { get; set; } = default(List<string>);

        public WeiXinMsgType msgType { get; set; }

        public FileType fileType { get; set; }

        public List<MessageAttachmentsDto> attachments { get; set; } = default(List<MessageAttachmentsDto>);
    }
}
