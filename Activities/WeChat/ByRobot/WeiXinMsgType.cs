using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat
{

    public enum WeiXinMsgType
    {
        /// <summary>
        /// 文本消息
        /// </summary>
        Text,

        /// <summary>
        /// markdown类型
        /// </summary>

        MarkDown,

        /// <summary>
        /// 图片类型
        /// </summary>
        Image,

        /// <summary>
        /// 图文类型
        /// </summary>
        News,
        /// <summary>
        /// 文件类型
        /// </summary>
        File,
        /// <summary>
        /// 模版卡片类型
        /// </summary>
        TemplateCard,

        MPNNews
    }

    public enum FileType
    {
        /// <summary>
        /// 图片
        /// </summary>
        image,

        /// <summary>
        /// 语音
        /// </summary>
        voice,

        /// <summary>
        /// 视频
        /// </summary>
        video,

        /// <summary>
        /// 文件，目前只有file有效，其余都不行
        /// </summary>
        file
    }

    public enum DingTalkMsgType
    {
        Text,
        Link,
        MarkDown,
        ActionCard,
        FeedCard
    }

}
