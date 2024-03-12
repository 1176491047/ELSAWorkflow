using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elsa;
using Elsa.Activities.Http.Models;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services;
using Elsa.Services.Models;
using Namotion.Reflection;
using Newtonsoft.Json;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat
{
    /// <summary>
    /// 企业微信群消息推送
    /// </summary>
    [Activity(
        Category = "消息推送",
        DisplayName = "企业微信群聊推送",
        Description = "通过群聊机器人发送消息(支持图片等其它类型)",
        Outcomes = new[] { OutcomeNames.Done })]
    public class WeiChatActivityWithContentType : Activity
    {
        [ActivityInput(
            Label = "Url",
            Hint = "Robot interface address",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string Url { get; set; }

        [ActivityInput(
            Label = "收件人(手机号)",
            UIHint = ActivityInputUIHints.MultiText,
            DefaultSyntax = SyntaxNames.Json,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript })]
        public ICollection<string> Receivers { get; set; } = new List<string>();


        [ActivityInput(
            Label = "收件人(用户ID)",
            UIHint = ActivityInputUIHints.MultiText,
            DefaultSyntax = SyntaxNames.Json,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript })]
        public ICollection<string> ReceiversOfUserId { get; set; } = new List<string>();
        //[ActivityInput(
        //    Label = "Request Params",
        //    Hint = "Receive parameters from outside,json format",
        //    SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public WeChatRequestDto SendParams { get; set; }

        [ActivityOutput]
        public string OutMessage { get; set; }



        [ActivityInput(
            Label = "内容",
            Hint = "消息推送主体",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string Content { get; set; }


        [ActivityInput(Hint = "内容类型", UIHint = ActivityInputUIHints.Dropdown,
      Options = new[] { WeiXinMsgType.Text, WeiXinMsgType.Image, WeiXinMsgType.MarkDown },
      DefaultValue = WeiXinMsgType.Text)]
        //内容类型
        public WeiXinMsgType ContentType { get; set; }

        [ActivityInput(
            Label = "标题",
            Hint = "消息标题",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string Title { get; set; }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            //var t= context.GetInput<HttpRequestModel>()!.Body;
            if (string.IsNullOrWhiteSpace(Url))
                return Fault("流程中机器人url为空，请配置");


            var result = WeiXinHelper.SendWeiChatRobotMessage(
                msgType: (int)ContentType,
                url: Url,
                title: Title,
                content: Content,
                messageUrl: "",
                pictureUrl: "",
                fileType: (int)FileType.file,
                 Receivers.ToList(),
                 ReceiversOfUserId.ToList()
            );

            await Task.Delay(TimeSpan.FromMilliseconds(1));
            OutMessage = JsonConvert.SerializeObject(result);
            return Done();
            
        }


        public override ValueTask<bool> CanExecuteAsync(ActivityExecutionContext context)
        {
            return new ValueTask<bool>(!string.IsNullOrWhiteSpace(Url) && SendParams != null);
        }

    }
}
