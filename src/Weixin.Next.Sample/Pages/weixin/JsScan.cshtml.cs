using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Weixin.Next.MP.Api;
using Weixin.Next.Sample.Models;

namespace Weixin.Next.Sample.Pages
{
    public class JsScanModel : PageModel
    {
        private readonly MpSettings _settings;
        private readonly IJsapiTicketManager _jsapi;

        public JsScanModel(IOptions<MpSettings> settings, IJsapiTicketManager jsapi)
        {
            _settings = settings.Value;
            _jsapi = jsapi;
        }

        public string WxConfig { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            //请先登录微信公众平台进入“公众号设置”的“功能设置”里填写“JS接口安全域名”

            var appId = _settings.AppId;
            var config = await JsConfig.Generate(_jsapi, appId, GetRequestUrl(),
                new[]
                {
                    JsApi.scanQRCode
                },
                true).ConfigureAwait(false);
            WxConfig = JsonConvert.SerializeObject(config);

            return Page();
        }

        private string GetRequestUrl()
        {
            return $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}{Request.QueryString}";
        }
    }
}