using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.MessageHandling
{
    public class MessageInfo
    {

        public string MessageBody { get; set; }

        /// <summary>
        /// text image
        /// </summary>
        public string MessageBodyType { get; set; }
        public string MessageAttachments { get; set; }
        public string AttachmentsFileType { get; set; }
        public string FileInfo { get; set; }
    }
}
