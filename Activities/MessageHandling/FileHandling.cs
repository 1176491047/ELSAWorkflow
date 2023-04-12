using Elsa;
using Elsa.Activities.Http.Models;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Services;
using Elsa.Services.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.MessageHandling
{
    public class FileHandling : Activity
    {
        [ActivityInput(Hint = "Source data type", UIHint = ActivityInputUIHints.Dropdown,
            Options = new[] { "BASE64", "Stream", "Byte[]", "object" },
            DefaultValue = "BASE64")]
        public string  DataType{ get; set; }
        protected override IActivityExecutionResult OnExecute(ActivityExecutionContext context)
        {
            var sourceActivity = context.WorkflowExecutionContext.WorkflowBlueprint.Connections.Where(x => x.Target.Activity.Id == context.ActivityId).FirstOrDefault().Source.Activity;

            //端点类型触发 第三方调用
            if (sourceActivity.Type == "HttpEndpoint")
            {
                res resust = new res();
                HttpRequestModel request = (HttpRequestModel)context.Input;
                EndPointRequestBody endPointRequestBody = JsonConvert.DeserializeObject<EndPointRequestBody>(request.Body.ToString());

                //如果包号附件
                if (endPointRequestBody.WithAttachments)
                {
                    resust.response = request.Body.ToString();

                    byte[][] lastresult = new byte[endPointRequestBody.AttachmentsInfo.Length][];
                    if (DataType == "BASE64")
                    {
                        for (int i = 0; i < endPointRequestBody.AttachmentsInfo.Length; i++)
                        {
                            string attachment = endPointRequestBody.AttachmentsInfo[i];
                            if (!string.IsNullOrEmpty(attachment))
                            {
                                string strbase64 = attachment.Trim().Substring(attachment.IndexOf(",") + 1);   //将‘，’以前的多余字符串删除
                                MemoryStream stream = new MemoryStream(Convert.FromBase64String(strbase64));
                                byte[] b = stream.ToArray();
                                lastresult[i] = b;
                                resust.base64str = strbase64;
                            }
                        }
                        resust.bytes = lastresult;


                        byte[][] imglastresult = new byte[endPointRequestBody.MessageBodyOfImage.Length][];
                        for (int i = 0; i < endPointRequestBody.MessageBodyOfImage.Length; i++)
                        {
                            string attachment = endPointRequestBody.MessageBodyOfImage[i];
                            if (!string.IsNullOrEmpty(attachment))
                            {
                                string strbase64 = attachment.Trim().Substring(attachment.IndexOf(",") + 1);   //将‘，’以前的多余字符串删除
                                MemoryStream stream = new MemoryStream(Convert.FromBase64String(strbase64));
                                byte[] b = stream.ToArray();
                                imglastresult[i] = b;
                                resust.imageBase64str = strbase64;
                            }
                        }
                        resust.imageBytes = imglastresult;
                    }
                }
                else
                {
                    resust.response = request.Body.ToString();
                }
                return Done(resust);
            }
            //请求类型触发 调用第三方
            else if (sourceActivity.Type == "SendHttpRequest")
            {

                res resust = new res();

                //获取当前节点的源节点
                var perActivityId = context.WorkflowExecutionContext.WorkflowBlueprint.Connections.Where(x => x.Target.Activity.Id == context.ActivityId).FirstOrDefault().Source.Activity.Id;

                var value = context.WorkflowInstance.ActivityData[perActivityId]["ResponseContent"].ToString();

                if (DataType == "BASE64")
                {
                    string strbase64 = value.Trim().Substring(value.IndexOf(",") + 1);   //将‘，’以前的多余字符串删除
                    MemoryStream stream = new MemoryStream(Convert.FromBase64String(strbase64));
                    byte[] b = stream.ToArray();
                    byte[][] lastresult = new byte[1][];
                    lastresult[0] = b;
                    resust.bytes = lastresult;
                    resust.base64str = value;
                    return Done(resust);
                }
                else if (DataType == "Byte[]")
                {
                    string strbase64 = value.Trim().Substring(value.IndexOf(",") + 1);   //将‘，’以前的多余字符串删除
                    byte[] b = Convert.FromBase64String(strbase64.Replace("\"", ""));
                    byte[][] lastresult = new byte[1][];
                    lastresult[0] = b;

                    resust.bytes = lastresult;
                    resust.base64str = value;
                    //resust.base64str = $@"<html><body leftMargin=10 topMargin=10 rightMargin=10 bottomMargin=10><div><img src='data:image/png;base64,{base64String}'></div></body></html>";
                    return Done(resust);
                }
                else if (DataType== "object")
                {
                    resust.str = value;
                    resust.response = value;
                    return Done(resust);
                }
                else
                {
                    return Suspend();
                }
            }
            else
            {
                    return Suspend();
            }
        }

        /// <summary>
        /// 恢复时
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override IActivityExecutionResult OnResume(ActivityExecutionContext context)
        {
            // Read received input.
            // Instruct workflow runner that we're done.
            return Done();
        }
    }

    public class res
    {
        public byte[][] bytes { get; set; }
        public byte[][] imageBytes { get; set; }
        public string base64str { get; set; }
        public string str { get; set; }
        public string imageBase64str { get; set; }
        public string response { get; set; }

    }

    public class EndPointRequestBody {

        public string MessageTitle { get; set; }
        public string MessageHead { get; set; }
        public string[] MessageBodyOfText { get; set; }
        public string[] MessageBodyOfImage { get; set; }
        public bool WithAttachments { get; set; }
        public string[] AttachmentsInfo { get; set; }

    }
}
