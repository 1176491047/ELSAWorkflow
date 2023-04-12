
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Nancy.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ElsaQuickstarts.Server.DashboardAndServer.Common.SendWeChat
{
    public class QYWeixinHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        ISession _session => _httpContextAccessor.HttpContext.Session;
        private IConfiguration _configuration;


        public QYWeixinHelper(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 获取企业微信的accessToken
        /// </summary>
        /// <param name="corpid">企业微信ID</param>
        /// <param name="corpsecret">管理组密钥</param>
        /// <returns></returns>
        public  string GetQYAccessToken()
        {
            string corpid = _configuration.GetSection("WeChat")["corpid"];
            string corpsecret = _configuration.GetSection("WeChat")["corpsecret"];
            string getAccessTokenUrl = _configuration.GetSection("WeChat")["getAccessTokenUrl"];
            string accessToken = "";
            string respText = "";



            if (_session.Keys.Contains("wechattoken"))
            {
                string token = _session.GetString("wechattoken");
                if (!string.IsNullOrEmpty(token))
                {
                    return token;
                }
            }


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
                if (getTokenResult.errcode== "0")
                {

                    accessToken = getTokenResult.access_token;
                    _session.SetString("wechattoken", accessToken);
                }

                //JavaScriptSerializer Jss = new JavaScriptSerializer();
                //Dictionary<string, object> respDic = (Dictionary<string, object>)Jss.DeserializeObject(respText);
                ////通过键access_token获取值
                //accessToken = respDic["access_token"].ToString();
                //_session.SetString("wechattoken", accessToken);
            }
            catch (Exception) { }

            return accessToken;
        }

        /// <summary>
        /// Post数据接口
        /// </summary>
        /// <param name="postUrl">接口地址</param>
        /// <param name="paramData">提交json数据</param>
        /// <param name="dataEncode">编码方式</param>
        /// <returns></returns>
        static string PostWebRequest(string postUrl, string paramData, Encoding dataEncode)
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
                return ex.Message;
            }
            return ret;
        }


      public string uploadFileToWechat(byte[] byteArray)
        {
            string ret = string.Empty;
            try
            {
                string postUrl = $"{_configuration.GetSection("WeChat")["fileuploadurl"]}?access_token={GetQYAccessToken()}&debug=1";


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
                    var fileHeader = "Content-Disposition: form-data; name=\"media\";filename=\"wework.txt\"; filelength=6\r\n" +
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
                            if (!string.IsNullOrWhiteSpace(result) && result.Trim().ToLower() == "success")
                            { }

                        }
                    }




                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return ret;
        }




        /// <summary>
        /// 推送信息
        /// </summary>
        /// <param name="corpsecret">管理组密钥</param>
        /// <param name="paramData">提交的数据json</param>
        /// <param name="dataEncode">编码方式</param>
        /// <returns></returns>
        public  void SendText(string empCode, string message)
        {
            string messageSendURI = _configuration.GetSection("WeChat")["messageSendURI"];
            string agentid = _configuration.GetSection("WeChat")["agentid"];


            string accessToken = "";
            string postUrl = "";
            string param = "";
            string postResult = "";


            accessToken = GetQYAccessToken();
            postUrl =$"{messageSendURI}?access_token={accessToken}";
            CorpSendText paramData = new CorpSendText(message, agentid);
            foreach (string item in empCode.Split('|'))
            {
                //paramData.touser = GetOAUserId(item);//在实际应用中需要判断接收消息的成员是否在系统账号中存在。
                paramData.touser = item;
                param = JsonConvert.SerializeObject(paramData);
                if (paramData.touser != null)
                {
                    postResult = PostWebRequest(postUrl, param, Encoding.UTF8);
                }
                else
                {
                    postResult = "账号" + paramData.touser + "在OA中不存在!";
                }
                //CreateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss") + ":\t" + item + "\t" + param + "\t" + postResult);
            }
        }

        private static void CreateLog(string strlog)
        {
            string str1 = "QYWeixin_log" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            //BS CS应用日志自适应
            string path = $"/{str1}";
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = Path.Combine(path, str1);
                StreamWriter sw = File.AppendText(path);
                sw.WriteLine(strlog);
                sw.Flush();
                sw.Close();

            }
            catch
            {
            }
        }

    }
}
