using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elsa;
using Elsa.Activities.Http.Models;
using Elsa.ActivityResults;
using Elsa.Attributes;
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
        Category = "Message Push",
        Description = "Enterprise WeChat robot settings",
        Outcomes = new[] { OutcomeNames.Done })]
    public class WeiChatActivity : Activity
    {
        [ActivityInput(
            Label = "Url",
            Hint = "Robot interface address",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string Url { get; set; }

        [ActivityInput(
            Label = "Receivers",
            Hint = "Message Recipients,Separate with commas, e.g.:user1,user2",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string Receivers { get; set; }


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


            var receivers = string.IsNullOrWhiteSpace(Receivers)
                ? new List<string>()
                : Receivers.Split(",").ToList();

            var result= WeiXinHelper.SendWeiChatRobotMessage(
                msgType: (int)WeiXinMsgType.Text,
                url: Url,
                title: Title,
                content:Content, 
                messageUrl: "", 
                pictureUrl: "",
                fileType:(int)FileType.file,
                 receivers
            );

            await Task.Delay(TimeSpan.FromMilliseconds(1));
            OutMessage =JsonConvert.SerializeObject(result) ;
            return Done();
           
        }


        public override ValueTask<bool> CanExecuteAsync(ActivityExecutionContext context)
        {
            return new ValueTask<bool>( !string.IsNullOrWhiteSpace(Url) && SendParams != null);
        }
       
    }
}
