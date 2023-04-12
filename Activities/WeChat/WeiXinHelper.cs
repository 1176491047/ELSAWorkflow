using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Text;
using JetBrains.Annotations;
using ElsaQuickstarts.Server.DashboardAndServer.Common;
using System.Security.Cryptography;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat
{
    /// <summary>
    /// 微信帮助类
    /// </summary>
    public class WeiXinHelper
    {
        /// <summary>
        /// 发送微信机器人消息
        /// </summary>
        /// <param name="msgType">发送类型</param>
        /// <param name="receivers">接收人</param>
        /// <param name="url">发送地址</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <param name="messageUrl">消息连接</param>
        /// <param name="pictureUrl">图片地址</param>
        /// <param name="messageAttachmentsDto">附件</param>
        /// <param name="fileType">文件类型</param>
        /// <returns></returns>
        public static HttpResponseResult SendWeiChatRobotMessage(int msgType,
            string url, string title, string content, string messageUrl, string pictureUrl,
            int fileType, List<string> receivers = null, List<MessageAttachmentsDto> messageAttachmentsDto = null)
        {
            HttpResponseResult res = new();

            try
            {
                if (!string.IsNullOrWhiteSpace(title))
                    title = Utils.WeiXinHtmlToText(title);

                if (!string.IsNullOrWhiteSpace(content))
                    content = Utils.WeiXinHtmlToText(content);

                //消息通知
                var at = (receivers == null || receivers.Count == 0) ? new List<string> { "@all" } : receivers;

                //创建一个HTTP请求  
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //Post请求方式  
                request.Method = "POST";
                //内容类型
                request.ContentType = "application/json";

                #region 添加消息类型和内容

                var paras = EncapsulateSendParams(msgType, title, content, messageUrl, pictureUrl, at);

                #endregion

                byte[] payload;
                //将Json字符串转化为字节  
                payload = Encoding.UTF8.GetBytes(paras);
                //设置请求的ContentLength   
                request.ContentLength = payload.Length;
                //发送请求，获得请求流 

                Stream writer;
                try
                {
                    writer = request.GetRequestStream();//获取用于写入请求数据的Stream对象
                }
                catch (Exception e)
                {
                    writer = null;
                    throw new Exception($"连接服务器失败：{e.Message}");
                }
                //将请求参数写入流
                writer.Write(payload, 0, payload.Length);
                //关闭请求流
                writer.Close();
                // String strValue = "";//strValue为http响应所返回的字符流
                HttpWebResponse response;
                try
                {
                    //获得响应流
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }
                Stream s = response.GetResponseStream();
                //  Stream postData = Request.InputStream;
                StreamReader sRead = new StreamReader(s);
                var postContent = JsonConvert.DeserializeObject<dynamic>(sRead.ReadToEnd());
                sRead.Close();
                res.Code = postContent.errcode;
                res.Success = postContent.errcode == 0;
                res.Msg = res.Success ? "发送成功" : postContent.errmsg;

                //消息推送成功，推送附件
                if (res.Success && messageAttachmentsDto != null && messageAttachmentsDto.Count > 0)
                {
                    var list = UploadFiles(url, messageAttachmentsDto, fileType);
                    //Console.WriteLine(JsonConvert.SerializeObject(list));
                    if (list != null && list.Count > 0)
                    {
                        //发送企业微信
                        SendFiles(url, list);
                    }
                }
            }
            catch (Exception e)
            {
                res.Msg = e.Message;
            }
            return res;
        }

        private static string EncapsulateSendParams(int msgType, string title, string content, string messageUrl,
            string pictureUrl, List<string> at)
        {
            var paras = string.Empty;
            switch (msgType)
            {

                case (int)WeiXinMsgType.Image:
                    {
                        dynamic obj = new ExpandoObject();
                        obj.msgtype = "image";
                        obj.image = new ExpandoObject();
                        obj.image.base64 = content;
                        obj.image.md5 = base64ToMD5(content);
                        //obj.text.mentioned_list = at.ToArray();
                        //设置参数
                        paras = JsonConvert.SerializeObject(obj);
                    }
                    break;

                case (int)WeiXinMsgType.Text:
                    {
                        dynamic obj = new ExpandoObject();
                        obj.msgtype = "text";
                        obj.text = new ExpandoObject();
                        obj.text.content = content;
                        //obj.text.mentioned_list = at.ToArray();
                        obj.text.mentioned_mobile_list = at.ToArray();
                        //设置参数
                        paras = JsonConvert.SerializeObject(obj);
                    }
                    break;
                case (int)WeiXinMsgType.MarkDown:
                    {
                        dynamic obj = new ExpandoObject();
                        obj.msgtype = "markdown";
                        obj.markdown = new ExpandoObject();
                        obj.markdown.content = content;
                        //设置参数
                        paras = JsonConvert.SerializeObject(obj);
                    }
                    break;
                case (int)WeiXinMsgType.News:
                    {
                        dynamic obj = new ExpandoObject();
                        obj.msgtype = "news";
                        obj.news = new ExpandoObject();
                        obj.news.articles = new ExpandoObject();
                        obj.news.articles.title = title;
                        obj.news.articles.description = content;
                        obj.news.articles.url = messageUrl;
                        obj.news.articles.picurl = pictureUrl;
                        //设置参数
                        paras = JsonConvert.SerializeObject(obj);
                    }
                    break;
                case (int)WeiXinMsgType.File:
                    break;
                case (int)WeiXinMsgType.TemplateCard:
                    {
                        dynamic obj = new ExpandoObject();
                        obj.msgtype = "template_card";
                        obj.template_card = new ExpandoObject();
                        obj.template_card.card_type = "text_notice";
                        obj.template_card.source = new ExpandoObject();
                        obj.template_card.source.icon_url = "https://wework.qpic.cn/wwpic/252813_jOfDHtcISzuodLa_1629280209/0";
                        obj.template_card.source.desc = "企业微信";
                        obj.template_card.source.desc_color = 0;
                        obj.template_card.main_title = new ExpandoObject();
                        obj.template_card.main_title.title = title;
                        obj.template_card.sub_title_text = content;
                        //obj.template_card.horizontal_content_list = new List<>() { }; //new ExpandoObject();
                        //"horizontal_content_list":[
                        //    {
                        //                            "keyname":"企微下载",
                        //        "value":"企业微信.apk",
                        //        "type":2,
                        //        "media_id":"MEDIAID"
                        //    }
                        //],
                        obj.template_card.card_action = new ExpandoObject();
                        obj.template_card.type = 1;
                        obj.template_card.url = "https://work.weixin.qq.com/?from=openApi";
                        //设置参数
                        paras = JsonConvert.SerializeObject(obj);
                    }
                    break;
                default:
                    return paras;
            }

            return paras;
        }


        private static string base64ToMD5(string base64Str) {
            byte[] bytes = Convert.FromBase64String(base64Str);
            byte[] md5Bytes = MD5.Create().ComputeHash(bytes);
            string md5Str = BitConverter.ToString(md5Bytes).Replace("-", "").ToLower();
            return md5Str;
        }

        /// <summary>
        /// 上传文件到微信
        /// </summary>
        /// <param name="url"></param>
        /// <param name="messageAttachmentsDto"></param>
        /// <param name="fileType"></param>
        public static List<string> UploadFiles(string url, List<MessageAttachmentsDto> messageAttachmentsDto, int fileType)
        {
            var list = new List<string>();
            //获取key值
            var index = url.IndexOf("key=");
            if (index != -1)
                url = url[index..];
            //上传文件地址
            var uploadUrl = $"https://qyapi.weixin.qq.com/cgi-bin/webhook/upload_media?{url}" +
                            $"&type={Enum.Parse(typeof(FileType), fileType.ToString())!.ToString()!.ToLower()}";
            //Console.WriteLine(uploadUrl);
            foreach (var item in messageAttachmentsDto)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uploadUrl);
                request.Method = "POST";
                string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线
                string startboundary = $"--{boundary}";
                string endboundary = $"--{boundary}--";
                request.ContentType = $"multipart/form-data; boundary={boundary}";
                Stream requestStream = request.GetRequestStream();
                //开始结束的换行符不能少，否则是44001,"errmsg":"empty media data, 
                byte[] endBoundaryBytes = Encoding.UTF8.GetBytes($"\r\n{endboundary}\r\n");
                //结束的两个换行符不能少，否则是44001,"errmsg":"empty media data,
                string fileTemplate = "Content-Disposition: form-data; name=\"{0}\";filename=\"{1}\"; filelength={2}\r\nContent-Type: {3}\r\n\r\n";
                byte[] fileBytes = Convert.FromBase64String(item.Base64String);
                StringBuilder sb = new StringBuilder();
                sb.Append(startboundary);
                sb.Append("\r\n");
                sb.Append(string.Format(fileTemplate, "media", item.FileName, fileBytes.Length, "application/octet-stream"));
                // LogInfo.Error("sb.ToString()=" + sb.ToString());
                byte[] Content = Encoding.UTF8.GetBytes(sb.ToString());
                //开始标志
                requestStream.Write(Content, 0, Content.Length);
                //文件内容
                requestStream.Write(fileBytes, 0, fileBytes.Length);
                //结束标志
                requestStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                // LogInfo.Error("endBoundaryBytes=" + endboundary);
                requestStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Encoding encoding = Encoding.GetEncoding(response.CharacterSet);
                //return GetResponseAsString(rsp, encoding);

                using Stream responseStream = response.GetResponseStream();
                using StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();
                Console.WriteLine(retString);
                myStreamReader.Close();
                var postContent = JsonConvert.DeserializeObject<dynamic>(retString);
                if (postContent.errcode == 0)
                {
                    Console.WriteLine(postContent.media_id);
                    list.Add(postContent.media_id.ToString());
                }
            }
            return list;
        }

        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="mediaIds"></param>
        public static void SendFiles(string url, List<string> mediaIds)
        {
            foreach (var mediaId in mediaIds)
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.ContentType = "application/octet-stream";//设置对应的ContentType
                string postdata = "{\"msgtype\": \"file\",\"file\": {\"media_id\":\"" + mediaId + "\"}}";
                byte[] data = Encoding.UTF8.GetBytes(postdata);//把字符串转换为字节

                Stream writer = req.GetRequestStream();//获取
                writer.Write(data, 0, data.Length);
                writer.Flush();
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();//获取服务器返回的结果
                Stream getStream = response.GetResponseStream();
                using StreamReader streamreader = new StreamReader(getStream);
                string result = streamreader.ReadToEnd();
                Console.WriteLine(result);
                streamreader.Close();
            }
        }
    }
}
