using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using DotLiquid;
using Elsa;
using Elsa.Activities.Http.Models;
using Elsa.Activities.Primitives;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Models;
using Elsa.Services;
using Elsa.Services.Models;
using ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool;
using ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool.Models;
using ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat;
using ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat.ByApplication;
using ElsaQuickstarts.Server.DashboardAndServer.Common;
using Microsoft.Extensions.Configuration;
using Namotion.Reflection;
using Newtonsoft.Json;
using Rebus.Messages;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage
{
    /// <summary>
    /// 企业微信群消息推送
    /// </summary>
    [Activity(
        Category = "消息推送",
        DisplayName = "钉钉群聊推送",
        Description = "通过群聊机器人发送消息",
        Outcomes = new[] { OutcomeNames.Done })]
    public class DingdingMessageActivity : Activity
    {
        private Logger aLogger;
        private IConfiguration _configuration;
        public DingdingMessageActivity(IConfiguration configuration)
        {
            _configuration = configuration;
            aLogger = Logger.Default;
        }



            [ActivityInput(
         Label = "AccessToken",
         Hint = "配置机器人时WebHook地址中截取",
         SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string AccessToken { get; set; }

            [ActivityInput(
         Label = "Secret",
         Hint = "配置机器人时勾选安全设置(加签)时获取",
         SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string Secret { get; set; }

        [ActivityInput(
            Label = "收件人(手机号)",
            UIHint = ActivityInputUIHints.MultiText,
            DefaultSyntax = SyntaxNames.Json,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript })]
        public ICollection<string> Receivers { get; set; } = new List<string>();

        [ActivityInput(
          Label = "@ALL",
          SupportedSyntaxes = new[] { SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public bool AtAll { get; set; }

                [ActivityInput(
         Label = "聊天框标题",
         SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string Title { get; set; }

        [ActivityInput(
         Label = "文本内容",
         SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string TextContent { get; set; }


        [ActivityInput(
         Label = "图片内容(Base64)",
         SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string ImageContent { get; set; }

        [ActivityInput(
      Label = "内网查看图片",
      SupportedSyntaxes = new[] { SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public bool LocalImage { get; set; }


        [ActivityInput(
         Label = "图片名称（必须包含后缀名.png）",
         SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string ImageName { get; set; }

        [ActivityOutput]
        public string OutMessage { get; set; }
        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            try
            {
            //var t= context.GetInput<HttpRequestModel>()!.Body;
            if (string.IsNullOrWhiteSpace(AccessToken)|| string.IsNullOrWhiteSpace(Secret))
                return Fault("流程中机器人地址信息为空，请配置");

            var webHookUrl = new WebHookUrl()
            {
                AccessToken = AccessToken,
                Secret = Secret
            };


                //服务地址
                string serverUrl = _configuration.GetSection("ELSAServerUrl")["AgentUrl"];
                string DingdingUrl = _configuration.GetSection("Dingding")["DingtalkURL"];
                var text = new MarkdownMessage()
            {
                Title = Title
            };
            string textInfo = $"#### {TextContent} \n > ";

            if (!string.IsNullOrEmpty(ImageContent))
            {
                //保存到本地 
                //图片ID
                Guid imageId = Guid.NewGuid();
                //图片名称
                string imageName = imageId + "-" + ImageName;

                //保存图片 返回相对路径
                string saveResult_path = common.SaveImage(ImageContent, imageName);
                if (string.IsNullOrEmpty(saveResult_path))
                {
                    aLogger.Info($"图片保存失败：{ImageName}", "钉钉推送异常");
                }

                    //拼接请求地址用作图片查询
                    string url = $"{serverUrl}/readImage?imageName={imageName}";
                    //拼接网络路径 用作缩略图渲染
                    string slImagePath = $"{serverUrl}/Images/{imageName}";

                    aLogger.Info($"图片查看:{url}", "钉钉群聊推送图文日志");
                    aLogger.Info($"回调:{slImagePath}", "钉钉群聊推送图文日志");

                    if (LocalImage)
                    {
                        textInfo += $"###### {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}-{imageName} [查看]({url}) \n";
                    }
                    else
                    {
                        //拼接图片部分
                        textInfo += $"![screenshot]({slImagePath})\n > ";
                    }
            }

            text.Text = textInfo;

            if (AtAll)
            {
                text.At = new AtSetting() { IsAtAll = true };
            }
            else
            {
                text.At = new AtSetting()
                {
                    //收件人
                    AtMobiles = Receivers.ToList(),
                    IsAtAll=false
                };
            }

            var sendResult = await DingtalkClient.SendMessageAsync(webHookUrl.ToUrlString(DingdingUrl), text);
            OutMessage = JsonConvert.SerializeObject(sendResult);
            return Done();
            }
            catch (Exception ex)
            {
                aLogger.Info($"{ex}", "钉钉推送异常");
                return Fault(ex);
            }

        }
    }
}
