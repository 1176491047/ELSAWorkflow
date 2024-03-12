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
using ElsaQuickstarts.Server.DashboardAndServer.Common.SendWeChat;
using Microsoft.AspNetCore.Http;
using ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat.ByApplication;
using ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat.Common.Model;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.WeChat
{
    /// <summary>
    /// 微信帮助类
    /// </summary>
    public class WeiXinHelper
    {
        private static Logger aLogger = Logger.Default;
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
            int fileType, List<string> receivers = null, List<string> receivers_Id = null, List<MessageAttachmentsDto> messageAttachmentsDto = null)
        {
            HttpResponseResult res = new();

            try
            {
                if (!string.IsNullOrWhiteSpace(title))
                    title = Utils.WeiXinHtmlToText(title);

                if (!string.IsNullOrWhiteSpace(content)&& msgType!= (int)WeiXinMsgType.MarkDown)
                    content = Utils.WeiXinHtmlToText(content);

                //消息通知
                var atOfid = (receivers_Id == null || receivers_Id.Count == 0) ? new List<string> { } : receivers_Id;

                var at = (receivers == null || receivers.Count == 0) && (receivers_Id == null || receivers_Id.Count == 0)? new List<string> { "@all" } : receivers;



                //创建一个HTTP请求  
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //Post请求方式  
                request.Method = "POST";
                //内容类型
                request.ContentType = "application/json";

                #region 添加消息类型和内容

                var paras = EncapsulateSendParams(msgType, title, content, messageUrl, pictureUrl, at, atOfid);
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

                aLogger.Info($"发生异常{e}", "微信发送");
                res.Msg = e.Message;
            }
            return res;
        }

        private static string EncapsulateSendParams(int msgType, string title, string content, string messageUrl,
            string pictureUrl, List<string> at, List<string> atOfId=null)
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
                        //obj.image.mentioned_list = at.ToArray();
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
                        obj.text.mentioned_list = atOfId.ToArray();
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
                else
                {
                    aLogger.Info($"企业微信文件上传失败{JsonConvert.SerializeObject(postContent)}","企业微信");
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








        #region 发送到应用

        /// <summary>
        /// 获取企业微信的accessToken
        /// </summary>
        /// <param name="corpid">企业微信ID</param>
        /// <param name="corpsecret">管理组密钥</param>
        /// <returns></returns>
        public static string GetQYAccessToken(string corpid, string corpsecret, string getAccessTokenUrl)
        {
            string accessToken = "";
            string respText = "";
            //获取josn数据
            string url = $"{getAccessTokenUrl}?corpid={corpid}&corpsecret={corpsecret}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (Stream resStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(resStream, Encoding.Default);
                respText = reader.ReadToEnd();
                resStream.Close();
            }

            try
            {

                GetTokenResult getTokenResult = JsonConvert.DeserializeObject<GetTokenResult>(respText);
                if (getTokenResult.errcode == "0")
                {

                    accessToken = getTokenResult.access_token;
                }

                //JavaScriptSerializer Jss = new JavaScriptSerializer();
                //Dictionary<string, object> respDic = (Dictionary<string, object>)Jss.DeserializeObject(respText);
                ////通过键access_token获取值
                //accessToken = respDic["access_token"].ToString();
                //_session.SetString("wechattoken", accessToken);
            }
            catch (Exception ex) {
                aLogger.Info($"企业微信获取token发生异常{ex}", "企业微信异常");
            }

            return accessToken;
        }

        /// <summary>
        /// Post数据接口
        /// </summary>
        /// <param name="postUrl">接口地址</param>
        /// <param name="paramData">提交json数据</param>
        /// <param name="dataEncode">编码方式</param>
        /// <returns></returns>
        static MessageSendResult PostWebRequest(string postUrl, string paramData, Encoding dataEncode)
        {
            string ret = string.Empty;
            try
            {
                byte[] byteArray = dataEncode.GetBytes(paramData); //转化
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                webReq.ContentType = "application/x-www-form-urlencoded";

                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                newStream.Close();
            }
            catch (Exception ex)
            {
                aLogger.Info($"发出消息发生异常:{ex}", "企业微信Error");
                return null;
            }

            MessageSendResult result = JsonConvert.DeserializeObject<MessageSendResult>(ret);
            if (result.errcode!=0)
            {
                aLogger.Info($"消息发送失败 结果:{ret}", "企业微信Error");
            }
            return result;
        }

        /// <summary>
        /// 上传临时文件 用作消息推送
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static string uploadFileToWechat(string postUrl, byte[] byteArray, string fileName="file.txt")
        {
            string ret = string.Empty;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(postUrl, UriKind.RelativeOrAbsolute));
                // 边界符
                var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
                webRequest.Method = "POST";
                webRequest.Timeout = 60000;
                webRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                // 开始边界符
                var beginBoundary = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
                // 结束结束符
                var endBoundary = Encoding.ASCII.GetBytes("--" + boundary + "--\r\n");
                var newLineBytes = Encoding.UTF8.GetBytes("\r\n");
                using (var stream = new MemoryStream())
                {
                    // 写入开始边界符
                    stream.Write(beginBoundary, 0, beginBoundary.Length);
                    // 写入文件
                    var fileHeader = $"Content-Disposition: form-data; name=\"media\";filename=\"{fileName}\"; filelength=6\r\n" +
                                     "Content-Type: application/octet-stream\r\n\r\n";
                    var fileHeaderBytes = Encoding.ASCII.GetBytes(fileHeader);
                    stream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);
                    stream.Write(byteArray, 0, byteArray.Length);


                    // 写入字符串
                    //var keyValue = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n";

                    //stream.Write(beginBoundary, 0, beginBoundary.Length);
                    //stream.Write(byteArray, 0, byteArray.Length);
                    //foreach (string key in parameters.Keys)
                    //{
                    //    var keyValueBytes = Encoding.UTF8.GetBytes(string.Format(keyValue, key, parameters[key]));
                    //    stream.Write(beginBoundary, 0, beginBoundary.Length);
                    //    stream.Write(keyValueBytes, 0, keyValueBytes.Length);
                    //}
                    // 写入结束边界符
                    stream.Write(endBoundary, 0, endBoundary.Length);
                    webRequest.ContentLength = stream.Length;
                    stream.Position = 0;
                    var tempBuffer = new byte[stream.Length];
                    stream.Read(tempBuffer, 0, tempBuffer.Length);
                    using (Stream requestStream = webRequest.GetRequestStream())
                    {
                        requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                        using (var response = webRequest.GetResponse())
                        using (StreamReader httpStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        {
                            string result = httpStreamReader.ReadToEnd();

                            UploadFileResult uploadFIleResult = JsonConvert.DeserializeObject<UploadFileResult>(result);
                            if (uploadFIleResult.errcode==0)
                            {
                                ret = uploadFIleResult.media_id;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                aLogger.Info($"上传临时文件发生异常:{ex}", "企业微信异常");
                return ex.Message;
            }
            return ret;
        }




        /// <summary>
        /// 推送信息
        /// </summary>
        /// <param name="sendMessageRequestDto"></param>
        public static bool  SendMessage(SendMessageRequestDto sendMessageRequestDto)
        {

            try
            {
            string accessToken = GetQYAccessToken(sendMessageRequestDto.Corpid, sendMessageRequestDto.Corpsecret, sendMessageRequestDto.GetAccessTokenUrl);
            string postUrl = $"{sendMessageRequestDto.MessageSendURI}?access_token={accessToken}";
            string fileUploadURL = $"{sendMessageRequestDto.FileUploadUrl}?access_token={accessToken}";

            foreach (string item in sendMessageRequestDto.Receivers)
            {
                //文本类型
                if (sendMessageRequestDto.MessageType == WeiXinMsgType.Text)
                {

                    TextMessage paramData = new TextMessage(sendMessageRequestDto.MessageContent);
                    //收件人赋值
                    paramData.touser = item;
                    paramData.agentid = sendMessageRequestDto.Agentid;
                    string param = JsonConvert.SerializeObject(paramData);
                    MessageSendResult postResult = PostWebRequest(postUrl, param, Encoding.UTF8);
                }
                else if (sendMessageRequestDto.MessageType == WeiXinMsgType.Image)
                {


                    //图片类型默认传入参数为base64字符串
                    string base64String = sendMessageRequestDto.MessageContent;
                    byte[] bytes = Convert.FromBase64String(base64String);

                    //上传图片 获取mediaID
                    string mediaId = uploadFileToWechat(fileUploadURL+ "&type=image", bytes, sendMessageRequestDto.ImageFileName);
                        if (string.IsNullOrEmpty(mediaId)) {
                            aLogger.Info($"上传图片获取MediaId失败：{sendMessageRequestDto.ImageFileName}", "企业微信异常");
                            continue;
                        }

                    ImageMessage imageMessage = new ImageMessage(mediaId);
                    imageMessage.touser= item;
                    imageMessage.agentid = sendMessageRequestDto.Agentid;

                    string param = JsonConvert.SerializeObject(imageMessage);
                    MessageSendResult postResult = PostWebRequest(postUrl, param, Encoding.UTF8);
                    }
                    else if (sendMessageRequestDto.MessageType == WeiXinMsgType.MarkDown)
                    {
                        MarkDownMessage markDownMessage = new MarkDownMessage(sendMessageRequestDto.MessageContent);
                        //收件人赋值
                        markDownMessage.touser = item;
                        markDownMessage.agentid = sendMessageRequestDto.Agentid;
                        string param = JsonConvert.SerializeObject(markDownMessage);
                        MessageSendResult postResult = PostWebRequest(postUrl, param, Encoding.UTF8);
                    }
                    else if (sendMessageRequestDto.MessageType == WeiXinMsgType.MPNNews)
                    {
                        //图片ID
                        Guid imageId = Guid.NewGuid();
                        //图片名称
                        string imageName = imageId+"-"+sendMessageRequestDto.ImageFileName;

                        //保存图片 返回相对路径
                        string saveResult_path = SaveImage(sendMessageRequestDto.MessageContent,imageName);
                        if (string.IsNullOrEmpty(saveResult_path))
                        {
                            aLogger.Info($"图片保存失败：{sendMessageRequestDto.ImageFileName}", "企业微信异常");
                            continue;
                        }

                        //拼接请求地址用作图片查询
                        string url =$"{sendMessageRequestDto.ServerUrl}/readImage?imageName={imageName}";
                        //拼接网络路径 用作缩略图渲染
                        string slImagePath = $"{sendMessageRequestDto.ServerUrl}/Images/{imageName}";

                        aLogger.Info($"图片查看:{url}", "企业微信个人推送图文日志");
                        aLogger.Info($"回调:{slImagePath}", "企业微信个人推送图文日志");
                        List<articles> articlesList = new List<articles>();
                        articles _articles = new articles();
                        //news
                        _articles.title = sendMessageRequestDto.NewsTitle;
                        //点击跳转的地址 用作查看报表详情
                        _articles.url = url;
                        //缩略图所需地址 
                        _articles.picurl = slImagePath;

                        //mpnnew需要用到的参数 测试
                        //_articles.thumb_media_id = mediaId;
                        //_articles.content_source_url = "www.baidu.com";
                        //_articles.content = $"<html>123<img src=\"data:image/png;base64,{base64String}\"/>4</html>";
                        //_articles.author = "张三";
                        articlesList.Add(_articles);
                        mpnews mpnews = new mpnews() { articles= articlesList };
                        NewsMessage  mPNMessage = new NewsMessage(mpnews);
                        mPNMessage.touser = item;
                        mPNMessage.agentid = sendMessageRequestDto.Agentid;
                        string param = JsonConvert.SerializeObject(mPNMessage);
                        MessageSendResult postResult = PostWebRequest(postUrl, param, Encoding.UTF8);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                aLogger.Info($"组装消息发生异常:{ex}", "企业微信异常");
                return false;
            }
        }

        private static string SaveImage(string base64String,string ImageName) {

            try
            { // 解码Base64数据为字节数组
                byte[] imageBytes = Convert.FromBase64String(base64String);
                string dateCode = DateTime.Now.ToShortDateString();
                //相对路径
                string parentPath = @$"{AppDomain.CurrentDomain.BaseDirectory}wwwroot\Images";
                if (!Directory.Exists(parentPath))
                {
                    Directory.CreateDirectory(parentPath);
                }
                string partPath = @$"{parentPath}\{ImageName}"; // 保存的路径和文件名
                // 创建文件流并写入字节数组
                using (FileStream fs = new FileStream(@$"{partPath}", FileMode.Create))
                {
                    fs.Write(imageBytes, 0, imageBytes.Length);
                }

                Console.WriteLine("图片已成功保存到本地文件夹。");
                return partPath;
            }
            catch (Exception ex)
            {

                aLogger.Info($"保存图片发生异常:{ex}", "企业微信异常");

                return "";
            }

        
        }


        #endregion
    }
}
