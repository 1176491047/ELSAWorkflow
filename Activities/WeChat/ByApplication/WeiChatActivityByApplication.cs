using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Elsa;
using Elsa.Activities.Http.Models;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services;
using Elsa.Services.Models;
using Microsoft.AspNetCore.Http;
using Namotion.Reflection;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using ElsaQuickstarts.Server.DashboardAndServer.Common;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat.ByApplication
{


    /// <summary>
    /// 企业微信以应用方式发送到个人 参考链接https://developer.work.weixin.qq.com/document/path/90236
    /// </summary>
    [Action(
        Category = "消息推送",
        DisplayName = "企业微信个人推送",
        Description = "通过应用方式推送个人消息",
        Outcomes = new[] { OutcomeNames.Done}
    )]
    public class WeiChatActivityByApplication : Activity
    {

        private Logger aLogger;

        private readonly IHttpContextAccessor _httpContextAccessor;
        ISession _session => _httpContextAccessor.HttpContext.Session;
        private IConfiguration _configuration;


        public WeiChatActivityByApplication(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            aLogger = Logger.Default;
        }


        [ActivityInput(
            Label = "应用ID",
            Hint = "为空使用则默认配置",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string Corpid { get; set; }



        [ActivityInput(
            Label = "应用密钥",
            Hint = "为空使用则默认配置",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string CorpSecret { get; set; }


        /// <summary>
        /// 收件人
        /// </summary>
        [ActivityInput(
            Label = "收件人",
            Hint = "收件人[首字母大写全拼 例:张三 ZhangSan]", 
            UIHint = ActivityInputUIHints.MultiText, 
            DefaultSyntax = SyntaxNames.Json, 
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript })]
        public ICollection<string> Receivers { get; set; } = new List<string>();



        /// <summary>
        /// 推送内容
        /// </summary>
        [ActivityInput(
            Label = "内容",
            Hint = "消息推送主体",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string Content { get; set; }

        /// <summary>
        /// 内容类型
        /// </summary>
        [ActivityInput(
            Label = "内容类型",
            Hint = "支持图片、文本(图片内容传入Base64字符串)", 
            UIHint = ActivityInputUIHints.Dropdown,
            Options = new[] { WeiXinMsgType.Text, WeiXinMsgType.Image, WeiXinMsgType.MarkDown },
            DefaultValue = WeiXinMsgType.Text)]
        //内容类型
        public WeiXinMsgType ContentType { get; set; }


        [ActivityInput(
            Label = "文件名称(英文)",
            Hint = "图片文件名称,必须包含后缀.[例:ProductionReport.PNG]",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string FileName { get; set; }

        /// <summary>
        /// 推送内容
        /// </summary>
        [ActivityInput(
            Label = "Title",
            Hint = "图文消息(MPNNews)使用的Title",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string Title { get; set; }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            string corpid = string.IsNullOrEmpty(Corpid)? _configuration.GetSection("WeChat")["corpid"]: Corpid; 
            string agentid =string.IsNullOrEmpty(CorpSecret)? _configuration.GetSection("WeChat")["agentid"]: CorpSecret; 
            string corpsecret = _configuration.GetSection("WeChat")["corpsecret"];
            string getAccessTokenUrl = _configuration.GetSection("WeChat")["getAccessTokenUrl"];
            string messageSendURI = _configuration.GetSection("WeChat")["messageSendURI"];
            string fileuploadurl = _configuration.GetSection("WeChat")["fileuploadurl"];
            string serverUrl = _configuration.GetSection("ELSAServerUrl")["AgentUrl"];

            if (string.IsNullOrEmpty(corpid) || string.IsNullOrEmpty(corpsecret) || string.IsNullOrEmpty(getAccessTokenUrl))
                return Fault("配置错误");

            SendMessageRequestDto sendMessageRequestDto = new SendMessageRequestDto()
            {
                Corpid = corpid,
                MessageSendURI = messageSendURI,
                Agentid= agentid,
                Corpsecret=corpsecret,
                GetAccessTokenUrl= getAccessTokenUrl,
                MessageContent= Content,
                MessageType= ContentType,
                Receivers = Receivers.ToList(),
                FileUploadUrl= fileuploadurl,
                ImageFileName= FileName,
                ServerUrl= serverUrl,
                NewsTitle= Title
            };

            WeiXinHelper.SendMessage(sendMessageRequestDto);
            return Done();
        }


        public override ValueTask<bool> CanExecuteAsync(ActivityExecutionContext context)
        {
            return new ValueTask<bool>(!string.IsNullOrWhiteSpace(Content) && Receivers.Count>0);
        }

    }
}
