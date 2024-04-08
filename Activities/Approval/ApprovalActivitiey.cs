using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elsa;
using Elsa.Activities.Email;
using Elsa.Activities.Email.Options;
using Elsa.Activities.Email.Services;
using Elsa.Activities.Http.Models;
using Elsa.Activities.Primitives;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Providers.WorkflowStorage;
using Elsa.Serialization;
using Elsa.Services;
using Elsa.Services.Models;
using ElsaQuickstarts.Server.DashboardAndServer.Activities.MessageHandling;
using ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat;
using ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat.ByApplication;
using ElsaQuickstarts.Server.DashboardAndServer.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NetBox.Extensions;
using Newtonsoft.Json;
using Storage.Net.Messaging;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.Approval
{
    /// <summary>
    /// 流程审批节点
    /// </summary>
    [Action(
        Category = "业务处理",
        DisplayName = "审批节点",
        Description = "流程审批节点",
            Outcomes = new string[0])]
    public class ApprovalActivitiey : Activity
    {
        private IConfiguration _configuration;
        private HttpClient _httpClient;
        private Logger aLogger;
        public ApprovalActivitiey(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            aLogger = Logger.Default;
        }

        private List<string> MeesageReceiver = new List<string>();

        #region 请求配置
        /// <summary>
        /// The HTTP method to use.
        /// </summary>
        [ActivityInput(
                Category = "请求配置",
                Hint = "选择HTTP method",
            UIHint = ActivityInputUIHints.Dropdown,
            Options = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS", "HEAD" },
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string? Method { get; set; } = "POST";

        [ActivityInput(
                Category = "请求配置",
            Label = "授权",
            Hint = "请求的Authorization标头值",
            SupportedSyntaxes = new[] { SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string? Authorization { get; set; }


        [ActivityInput(
                Category = "请求配置",
             Hint = "要随请求一起发送的其他标头",
             UIHint = ActivityInputUIHints.MultiLine, DefaultSyntax = SyntaxNames.Json,
             SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript, SyntaxNames.Liquid }
         )]
        public Elsa.Activities.Http.Models.HttpRequestHeaders RequestHeaders { get; set; } = new();

        [ActivityInput(
                Category = "请求配置",
         UIHint = ActivityInputUIHints.Dropdown,
         Hint = "请求的内容类型",
         Options = new[] { "", "text/plain", "text/html", "application/json", "application/xml", "application/x-www-form-urlencoded" },
         SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid }
     )]
        public string? ContentType { get; set; } = "application/json";

        #endregion


        #region 消息推送

        [ActivityInput(
          Label = "应用ID",
          Hint = "为空使用则默认配置",
          SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid },
            Category = "MessagePush")]
        public string Corpid { get; set; }



        [ActivityInput(
            Label = "应用密钥",
            Hint = "为空使用则默认配置",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid },
            Category = "MessagePush")]
        public string CorpSecret { get; set; }


        /// <summary>
        /// 收件人
        /// </summary>
        [ActivityInput(
            Label = "收件人",
            Hint = "收件人[首字母大写全拼 例:张三 ZhangSan]",
            UIHint = ActivityInputUIHints.MultiText,
            DefaultSyntax = SyntaxNames.Json,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript },
            Category = "MessagePush")]
        public ICollection<string> Receivers { get; set; } = new List<string>();

        /// <summary>
        /// 推送内容
        /// </summary>
        [ActivityInput(
            Label = "内容",
            Hint = "消息推送主体",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid },
            Category = "MessagePush")]
        public string Content { get; set; }


        /// <summary>
        /// 内容类型
        /// </summary>
        [ActivityInput(
            Label = "内容类型",
            Hint = "支持图片、文本(图片内容传入Base64字符串)",
            UIHint = ActivityInputUIHints.Dropdown,
            Options = new[] { WeiXinMsgType.Text, WeiXinMsgType.Image, WeiXinMsgType.MarkDown },
            DefaultValue = WeiXinMsgType.Text,
            Category = "MessagePush")]
        //内容类型
        public WeiXinMsgType MessageType { get; set; }


        [ActivityInput(
            Label = "文件名称(英文)",
            Hint = "图片文件名称,必须包含后缀.[例:ProductionReport.PNG]",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid },
            Category = "MessagePush")]
        public string FileName { get; set; }

        /// <summary>
        /// 推送内容
        /// </summary>
        [ActivityInput(
            Label = "Title",
            Hint = "图文消息(MPNNews)使用的Title",
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid },
            Category = "MessagePush")]
        public string Title { get; set; }

        #endregion





        [ActivityInput(
            Hint = "结果",
            UIHint = ActivityInputUIHints.MultiText,
            DefaultSyntax = SyntaxNames.Json,
            SupportedSyntaxes = new[] { SyntaxNames.Json },
            IsDesignerCritical = true,
            ConsiderValuesAsOutcomes = true
        )]
        public ISet<string> Outputs { get; set; } = new HashSet<string>();




        [ActivityInput(
            Label = "节点标识(唯一)",
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript })]
        public string? NodeKey { get; set; }


        [ActivityInput(
            Label = "直接完成(不会等待回执)",
            UIHint = ActivityInputUIHints.Checkbox)]
        public bool Finish { get; set; }=false;


        /// <summary>
        ///节点触发 调用第三方接口等待回调
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {

           string preNodekey= context.GetVariable("PreNodeKey")?.ToString();

            context.SetVariable($"{Guid.NewGuid()}", $"{context.ActivityBlueprint.Name},节点入参{context.Input.ToString()}");

            ApprovalInput approvalInput = new ApprovalInput();
            var sourceActivity = context.WorkflowExecutionContext.WorkflowBlueprint.Connections.Where(x => x.Target.Activity.Id == context.ActivityId).FirstOrDefault().Source.Activity;
            //端点类型触发 第三方调用
            if (sourceActivity.Type == "HttpEndpoint")
            {
                res resust = new res();
                HttpRequestModel requestModel = (HttpRequestModel)context.Input;
                //流程发起时取入参的收件人
                 approvalInput = JsonConvert.DeserializeObject<ApprovalInput>(JsonConvert.SerializeObject(requestModel.Body));

                context.SetVariable("BaseData", approvalInput);
                MeesageReceiver.AddRange(approvalInput.Receiver);
            }
            else
            {
                approvalInput = JsonConvert.DeserializeObject<ApprovalInput>(JsonConvert.SerializeObject(context.GetVariable("BaseData")));
                //由其它节点触发流程 取上个节点输入的收件人
                string PreReceiver = context.GetVariable("PreReceiver").ToString();
                MeesageReceiver.AddRange(JsonConvert.DeserializeObject<List<string>>(PreReceiver));
            }


            //调用安环接口
            //读取业务服务接口地址
            string serviceUrl = _configuration["AHServiceURL"];
            ApprovalOutPut approvalOutPut = new ApprovalOutPut()
            {
                CurrentNodeId = context.ActivityId,
                WorkflowInstanseId = context.WorkflowInstance.Id,
                CurrentNodeKey = NodeKey,
                PreviousNodeKey = preNodekey,
                CurrentNodeName = context.ActivityBlueprint.DisplayName,
                Code = approvalInput.Code,
                PreResult = context.GetVariable("PreNodeResult")?.ToString()
            };
            var request = CreateRequest(serviceUrl, JsonConvert.SerializeObject(approvalOutPut));
            await _httpClient.SendAsync(request);
            context.SetVariable($"{Guid.NewGuid()}", $"{context.ActivityBlueprint.DisplayName}节点触发");

            //加入页面配置到节点的用户
            MeesageReceiver.AddRange(Receivers.ToList());

            //发送企业微信消息
            await SendMessage();

            //直接完成类型的节点
            if (Finish)
                return Done();

            return Suspend();
        }



        private async Task SendMessage() {
            string corpid = string.IsNullOrEmpty(Corpid) ? _configuration.GetSection("WeChat")["corpid"] : Corpid;
            string agentid = string.IsNullOrEmpty(CorpSecret) ? _configuration.GetSection("WeChat")["agentid"] : CorpSecret;
            string corpsecret = _configuration.GetSection("WeChat")["corpsecret"];
            string getAccessTokenUrl = _configuration.GetSection("WeChat")["getAccessTokenUrl"];
            string messageSendURI = _configuration.GetSection("WeChat")["messageSendURI"];
            string fileuploadurl = _configuration.GetSection("WeChat")["fileuploadurl"];
            string serverUrl = _configuration.GetSection("ELSAServerUrl")["AgentUrl"];



            if (string.IsNullOrEmpty(corpid) || string.IsNullOrEmpty(corpsecret) || string.IsNullOrEmpty(getAccessTokenUrl))
                aLogger.Info($"缺少配置 消息发送失败", $"消息发送失败[ApprovalActivitiey]");

            SendMessageRequestDto sendMessageRequestDto = new SendMessageRequestDto()
            {
                Corpid = corpid,
                MessageSendURI = messageSendURI,
                Agentid = agentid,
                Corpsecret = corpsecret,
                GetAccessTokenUrl = getAccessTokenUrl,
                MessageContent = Content,
                MessageType = MessageType,
                Receivers = MeesageReceiver,
                FileUploadUrl = fileuploadurl,
                ImageFileName = FileName,
                ServerUrl = serverUrl,
                NewsTitle = Title
            };

            WeiXinHelper.SendMessage(sendMessageRequestDto);
        }


        /// <summary>
        /// 处理完成
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override IActivityExecutionResult OnResume(ActivityExecutionContext context)
        {
            context.SetVariable($"{Guid.NewGuid()}", $"{context.ActivityBlueprint.Name}节点恢复,输出：{context.Input.ToString()}");

            ApprovalInput approvalInput = JsonConvert.DeserializeObject<ApprovalInput>(context.Input.ToString());
            if (approvalInput==null||string.IsNullOrEmpty(approvalInput.Result))
                return Done();
            //当前节点结果 传给下个节点

            context.SetVariable("PreNodeResult", approvalInput.Result);
            //当前节点的key 传给下个节点
            context.SetVariable("PreNodeKey", NodeKey);
            //当前节点恢复时传入的收件人 传给下个节点
            context.SetVariable("PreReceiver", JsonConvert.SerializeObject(approvalInput.Receiver));
            return Outcome(approvalInput.Result.ToString());
        }

         private HttpRequestMessage CreateRequest(string url,object? content)
        {
            var method = Method ?? HttpMethods.Get;
            var methodSupportsBody = GetMethodSupportsBody(method);
            var request = new HttpRequestMessage(new HttpMethod(method), url);
            var authorizationHeaderValue = Authorization;
            var requestHeaders = new HeaderDictionary(RequestHeaders.ToDictionary(x => x.Key, x => new StringValues(x.Value.Split(','))));

            if (methodSupportsBody)
            {
                var bodyAsString = content as string;
                var bodyAsBytes = content as byte[];
                var contentType = ContentType;

                if (!string.IsNullOrWhiteSpace(bodyAsString))
                    request.Content = new StringContent(bodyAsString, Encoding.UTF8, contentType);
                else if (bodyAsBytes != null)
                {
                    request.Content = new ByteArrayContent(bodyAsBytes);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                }
            }

            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorizationHeaderValue);

            //foreach (var header in requestHeaders)
            //    request.Headers.Add(header.Key, header.Value.AsEnumerable());

            return request;
        }
        private static bool GetMethodSupportsBody(string method)
        {
            var methods = new[] { "POST", "PUT", "PATCH", "DELETE" };
            return methods.Contains(method, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
