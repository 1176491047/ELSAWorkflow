using System;
using System.Security.Cryptography;
using System.Text;


namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.DingdingMessage.CommonTool
{

    public class WebHookUrl
    {
        public string BaseUrl { get; set; } = "https://oapi.dingtalk.com/robot/send";

        public string AccessToken { get; set; }

        public string Secret { get; set; }

        public string ToUrlString(string baseUrl="")
        {
            if (!string.IsNullOrEmpty(baseUrl))
                BaseUrl = baseUrl;
            if (string.IsNullOrWhiteSpace(AccessToken))
                throw new ArgumentNullException(nameof(AccessToken), "AccessToken cannot be null");

            if (string.IsNullOrWhiteSpace(Secret))
                throw new ArgumentNullException(nameof(Secret), "AccessToken cannot be null");

            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timestamp = (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
            string stringToSign = $"{timestamp}\n{Secret}";
            string sign = ComputeHmacSha256(stringToSign, Secret);
            return $"{BaseUrl}?access_token={AccessToken}&sign={sign}&timestamp={timestamp}";
        }

        private static string ComputeHmacSha256(string message, string secretKey)
        {
            byte[] key = Encoding.UTF8.GetBytes(secretKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(key))
            {
                byte[] hashValue = hmac.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashValue);
            }
        }
    }
}
