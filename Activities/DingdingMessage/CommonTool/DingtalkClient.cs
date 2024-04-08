using ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool
{
    /// <summary>
    /// 钉钉群机器人Client端
    /// open定义地址
    /// https://open.dingtalk.com/document/orgapp/custom-robots-send-group-messages
    /// </summary>
    public class DingtalkClient
    {
        /// <summary>
        /// 异步发送文本信息
        /// </summary>
        /// <param name="webHookUrl">webhook的url地址</param>
        /// <param name="message">文本内容</param>
        /// <param name="AtMobiles">@的手机号列表</param>
        /// <param name="isAtAll">是否@所有人</param>
        /// <returns></returns>
        public static async Task<SendResult> SendMessageAsync(string webHookUrl, string message, List<string> AtMobiles = null, bool isAtAll = false)
        {
            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrEmpty(webHookUrl))
            {
                return new SendResult
                {
                    ErrMsg = "参数不正确",
                    ErrCode = -1
                };
            }
            var msg = new TextMessage
            {
                Content = message,
                At = new AtSetting()
            };
            if (AtMobiles != null)
            {
                msg.At.AtMobiles = AtMobiles;
                msg.At.IsAtAll = false;
            }
            else
            {
                if (isAtAll) msg.At.IsAtAll = true;
            }

            var json = msg.GetContent();
            return await SendAsync(webHookUrl, json);
        }


        /// <summary>
        /// 异步发送其他类型的信息
        /// </summary>
        /// <param name="webHookUrl">webhook的url地址</param>
        /// <param name="message">信息model</param>
        /// <returns></returns>
        public static async Task<SendResult> SendMessageAsync(string webHookUrl, IDingtalkMessage message)
        {
            if (message == null || string.IsNullOrEmpty(webHookUrl))
            {
                return new SendResult
                {
                    ErrMsg = "参数不正确",
                    ErrCode = -1
                };
            }
            var json = message.GetContent();
            return await SendAsync(webHookUrl, json);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="webHookUrl"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static async Task<SendResult> SendAsync(string webHookUrl, string data)
        {
            try
            {
                string result = string.Empty;
                WebRequest WReq = WebRequest.Create(webHookUrl);
                WReq.Method = "POST";
                byte[] byteArray = Encoding.UTF8.GetBytes(data);
                WReq.ContentType = "application/json; charset=utf-8";
                using (var newStream = await WReq.GetRequestStreamAsync())
                {
                    await newStream.WriteAsync(byteArray, 0, byteArray.Length);
                }
                WebResponse WResp = await WReq.GetResponseAsync();
                using (var stream = WResp.GetResponseStream())
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            result = await reader.ReadToEndAsync();
                        }

                    }
                }
                if (!string.IsNullOrEmpty(result))
                {
                    var re = JsonConvert.DeserializeObject<SendResult>(result);
                    return re;
                }
                return new SendResult { ErrMsg = "", ErrCode = -1 };
            }
            catch (Exception ex)
            {
                return new SendResult { ErrMsg = ex.Message, ErrCode = -1 };
            }
        }
    }
}
