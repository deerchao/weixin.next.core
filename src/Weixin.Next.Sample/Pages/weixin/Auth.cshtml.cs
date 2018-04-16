using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Weixin.Next.Common;
using Weixin.Next.MP.Api;
using Weixin.Next.Sample.Models;

namespace Weixin.Next.Sample.Pages
{
    public class AuthModel : PageModel
    {
        private readonly MpSettings _settings;

        public AuthModel(IOptions<MpSettings> settings)
        {
            _settings = settings.Value;
        }

        public string WxConfig { get; set; }

        private IActionResult Auth(string scope, string state)
        {
            var appId = _settings.AppId;

            //相对网址变为绝对网址
            var callbackUrl = Url.Page("Auth", "AuthCallback", null, Request.Scheme);

            var redirectUrl = OAuth2.GetAuthorizeUrl(appId, callbackUrl, scope, state);
            return Redirect(redirectUrl);
        }

        public IActionResult OnGetBase()
        {
            return Auth(OAuth2.ScopeBase, "base");
        }

        public IActionResult OnGetInfo()
        {
            return Auth(OAuth2.ScopeUserInfo, "userinfo");
        }

        /// <summary>
        /// 获取用户信息后被微信重定向回来的入口
        /// </summary>
        /// <param name="code">授权 code</param>
        /// <param name="state">之前重定向过去时网址中的 state 参数</param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAuthCallback(string code, string state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Content($"您拒绝了授权.");
            }

            var appId = _settings.AppId;
            var appSecret = _settings.AppSecret;

            OAuth2.AccessTokenResult tokenResult;
            try
            {
                tokenResult = await OAuth2.GetAccessToken(appId, appSecret, code).ConfigureAwait(false);
            }
            catch (ApiException ex)
            {
                return Content($"使用 code({code}) 换取 access_token 失败: {ex.Code} {ex.Message}");
            }

            //只有使用了 OAuth2.ScopeUserInfo (snsapi_userinfo) 为 scope 才能获取用户信息, 否则到此为止
            if (state != "userinfo")
                return Content("基本用户信息: " + JsonConvert.SerializeObject(tokenResult));

            try
            {
                var userResult = await OAuth2.GetUserInfo(tokenResult.openid, tokenResult.access_token);
                return Content("完整用户信息: " + JsonConvert.SerializeObject(userResult));
            }
            catch (ApiException ex)
            {
                return Content($"使用 access_token({tokenResult.access_token}) 换取用户({tokenResult.openid})信息失败: {ex.Code} {ex.Message}");
            }
        }


        private string GetRequestUrl()
        {
            return $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}{Request.QueryString}";
        }
    }
}