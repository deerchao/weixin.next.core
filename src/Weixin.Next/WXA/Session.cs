using System;
using System.Threading.Tasks;
using Weixin.Next.Common;

namespace Weixin.Next.WXA
{
    public class Session
    {
        /// <summary>
        /// 以小程序 wx.login 获得的 code 换取用户唯一标识 openid 和 会话密钥 session_key
        /// </summary>
        /// <param name="appId">小程序唯一标识</param>
        /// <param name="appSecret">小程序的 app secret</param>
        /// <param name="jsCode">登录时获取的 code</param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static async Task<SessionResult> Get(string appId, string appSecret, string jsCode, ApiConfig config = null)
        {
            var url = $"https://api.weixin.qq.com/sns/jscode2session?grant_type=authorization_code&appid={Uri.EscapeDataString(appId)}&secret={Uri.EscapeDataString(appSecret)}&js_code={jsCode}";
            var text = await ApiHelper.GetString(url, config).ConfigureAwait(false);
            return ApiHelper.BuildResult<SessionResult>(text);
        }

        public class SessionResult : IApiResult
        {
            /// <summary>
            /// 用户唯一标识
            /// </summary>
            public string openid { get; set; }

            /// <summary>
            /// 会话密钥
            /// </summary>
            public string session_key { get; set; }

            /// <summary>
            /// 用户在开放平台的唯一标识符
            /// </summary>
            public string unionid { get; set; }
        }
    }
}
