using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
namespace ElsaQuickstarts.Server.DashboardAndServer.Common
{

    /// <summary>
    /// 通用工具类
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// 钉钉Robot消息加密
        /// 加密规则
        /// 把timestamp+"\n"+密钥当做签名字符串，
        /// 使用HmacSHA256算法计算签名，
        /// 然后进行Base64 encode，
        /// 最后再把签名参数再进行urlEncode，得到最终的签名（需要使用UTF-8字符集）
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string EncryptWithSHA256(long timestamp, string secret)
        {
            string stringToSign = timestamp + "\n" + secret;
            var encoding = Encoding.UTF8;
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(stringToSign);
            using var hmacsha256 = new HMACSHA256(keyByte);
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return HttpUtility.UrlEncode(Convert.ToBase64String(hashmessage), encoding);
        }

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp()
        {
            long currenttimemillis = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            return currenttimemillis;
        }

        /// <summary>
        /// SQL注入过滤
        /// </summary>
        /// <param name="InText">要过滤的字符串</param>
        /// <returns>过滤掉后的字符串</returns>
        public static string SqlFilterString(string InText)
        {
            if (string.IsNullOrEmpty(InText))
            {
                return InText;
            }
            InText = Replace(InText, "[) ]((and)|(exec)|(insert)|(select)|(delete)|(drop)|(update)|(chr)|(mid)|(master)|(or)|(truncate)|(char)|(declare)|(join)|(count))[ (]", "");
            InText = Replace(InText, "['%@&*<>]", "");
            InText = Replace(InText, "([-]+)", "-");

            //InText = InText.ToLower();
            //string word = "and|exec|insert|select|delete|drop|update|chr|mid|master|or|truncate|char|declare|'|%|@|&|-|count|*|<|>|join";
            //foreach (string i in word.Split('|'))
            //{
            //    if ((InText.IndexOf(i + " ") > -1) || (InText.ToLower().IndexOf(" " + i) > -1))
            //    //这样可以让含有危险字符的字符串进行注册
            //    {
            //        InText = InText.Replace(i, "");
            //    }
            //}
            return InText;
        }

        /// <summary>
        /// 正则替换字符
        /// </summary>
        /// <param name="input">要搜索匹配项的字符串</param>
        /// <param name="pattern">要匹配的正则表达式模式</param>
        /// <param name="replacement">替换字符字符串</param>
        /// <returns></returns>
        public static string Replace(string input, string pattern, string replacement)
        {
            return Regex.Replace(input, pattern, replacement, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否匹配正则表达式
        /// </summary>
        /// <param name="input">要匹配的字符串</param>
        /// <param name="pattern">要匹配的正则表达式</param>
        /// <returns></returns>
        public static bool IsMatch(string input, string pattern)
        {
            Match match = Regex.Match(input, pattern);
            return match.Success;
        }

        /// <summary>
        /// 获取消息批次流水号
        /// </summary>
        /// <param name="sceneType">场景类型</param>
        /// <param name="channelType">渠道类型（支持多渠道）</param>
        /// <returns></returns>
        public static string GetMessageBatchNo(int sceneType, int channelType)
        {
            return sceneType + channelType + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 10000);
        }



        /// <summary>
        /// 是否为空集合
        /// </summary>
        /// <returns></returns>
        public static bool IsEmptyList<T>(List<T> lst)
        {
            return lst == null || lst.Count == 0 || lst.Where(x => !string.IsNullOrWhiteSpace(x.ToString())).ToList().Count == 0;
        }

        /// <summary>
        /// 模板占位符转换成字典
        /// </summary>
        /// <param name="input">要匹配的字符串</param>
        /// <param name="paramsDescription">模板参数说明字典</param>
        /// <param name="add">添加字典</param>
        /// <returns></returns>
        public static Dictionary<string, string> TemplatePlaceholderToDictionary(string input, Dictionary<string, string> paramsDescription, Dictionary<string, string> add)
        {
            var matchs = Regex.Matches(input, @"\{\w+\}", RegexOptions.IgnoreCase);
            if (matchs != null && matchs.Count > 0)
            {
                foreach (Match match in matchs)
                {
                    string key = match.Value.Replace("{", "").Replace("}", "");
                    if (add.ContainsKey(key))
                        continue;
                    string value = paramsDescription.ContainsKey(key) ? paramsDescription[key] : "";
                    add.Add(key, value);
                }
            }
            return add;
        }

        /// <summary>
        /// 解决附件中文名乱码问题
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="encoding">字符集编码</param>
        /// <returns></returns>
        public static string ConvertHeaderToBase64(string fileName, Encoding encoding)
        {
            //https://www.cnblogs.com/qingspace/p/3732677.html
            var encode = !string.IsNullOrEmpty(fileName) && fileName.Any(c => c > 127);
            if (encode)
            {
                return "=?" + encoding.WebName + "?B?" + Convert.ToBase64String(encoding.GetBytes(fileName)) + "?=";
            }
            return fileName;
        }

        /// <summary>
        /// 解决附件中文名乱码问题（UTF-8格式）
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public static string ConvertHeaderToBase64(string fileName)
        {
            return ConvertHeaderToBase64(fileName, Encoding.UTF8);
        }

        /// <summary>
        /// 获取模板需要替换的数据
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<string> GetReplaceTemplateData(string input)
        {
            var list = new List<string>();
            var matchs = Regex.Matches(input, @"(\{\w+[:]\w+\})", RegexOptions.IgnoreCase);
            if (matchs != null && matchs.Count > 0)
            {
                //提取匹配项
                foreach (Match match in matchs)
                {
                    list.Add(match.Value.Replace("{", "").Replace("}", ""));
                }
            }
            return list;
        }

        /// <summary>
        /// 获取SQL分隔符
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static char GetSQLSeparation(int dbType)
        {
            return "@:"[dbType];
        }

        /// <summary>
        ///  用于处理微信html格式转换文本格式
        /// </summary>
        /// <param name="input">待转换的HTML文本</param>
        /// <returns>纯文本</returns>
        public static string WeiXinHtmlToText(string input)
        {
            //去除空行
            input = Regex.Replace(input, @"\s", "");
            //去除注释
            input = Regex.Replace(input, @"<!--[\s\S]*?-->", "");
            //去掉<hr>
            input = Regex.Replace(input, @"<hr([\s\S]*?)>", "\n");
            //<br> 换成 \n
            input = Regex.Replace(input, @"<br/>", "\n");
            //<li> 换成 >
            input = Regex.Replace(input, @"<li([\s\S]*?)>", "\n ");
            input = Regex.Replace(input, @"</li>", "");
            //<div> 去掉
            input = Regex.Replace(input, @"<div([\s\S]*?)>", "");
            input = Regex.Replace(input, @"</div>", "");
            //<em> 去掉
            input = Regex.Replace(input, @"<em([\s\S]*?)>", "");
            input = Regex.Replace(input, @"</em>", "");
            //<ul> 去掉
            input = Regex.Replace(input, @"<ul([\s\S]*?)>", "");
            input = Regex.Replace(input, @"</ul>", "\n");
            //<address> 去掉
            input = Regex.Replace(input, @"<address([\s\S]*?)>", "");
            input = Regex.Replace(input, @"</address>", "\n");
            //<abbr> 去掉
            input = Regex.Replace(input, @"<abbr([\s\S]*?)>", "");
            input = Regex.Replace(input, @"</abbr>", "\n");
            //<p> 换成 \n
            input = Regex.Replace(input, @"<p([\s\S]*?)>", "\n");
            input = Regex.Replace(input, @"</p>", "");
            //<h> 换成 #
            input = Regex.Replace(input, @"<h1([\s\S]*?)>", "\n # ");
            input = Regex.Replace(input, @"<h2([\s\S]*?)>", "\n ## ");
            input = Regex.Replace(input, @"<h3([\s\S]*?)>", "\n ### ");
            input = Regex.Replace(input, @"<h4([\s\S]*?)>", "\n #### ");
            input = Regex.Replace(input, @"<h5([\s\S]*?)>", "\n ##### ");
            input = Regex.Replace(input, @"</h1>", "\n");
            input = Regex.Replace(input, @"</h2>", "\n");
            input = Regex.Replace(input, @"</h3>", "\n");
            input = Regex.Replace(input, @"</h4>", "\n");
            input = Regex.Replace(input, @"</h5>", "\n");
            //<strong> 换成**bold**
            input = Regex.Replace(input, @"<strong>", " **");
            input = Regex.Replace(input, @"</strong>", " **");

            var reg = @"<body>([\s\S]*?)</body>";
            var match = Regex.Match(input, reg, RegexOptions.IgnoreCase);
            if (match.Success)
                input = match.Value.Replace("<body>", "").Replace("</body>", "");
            return input;
        }
    }
}
