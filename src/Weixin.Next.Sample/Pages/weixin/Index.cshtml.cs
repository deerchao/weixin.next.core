using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Weixin.Next.MP.Messaging;
using Weixin.Next.Sample.Models;

namespace Weixin.Next.Sample.Pages
{
    public class WeixinModel : PageModel
    {
        private readonly MessageCenter _messageCenter;
        private readonly MpSettings _settings;

        public WeixinModel(IOptions<MpSettings> settings, MessageCenter messageCenter)
        {
            _settings = settings.Value;
            _messageCenter = messageCenter;
        }

        public IActionResult OnGet(string signature, string timestamp, string nonce, string echostr)
        {
            if (!string.IsNullOrEmpty(signature))
            {
                if (Signature.Check(_settings.Token, timestamp, nonce, signature))
                {
                    return Content(echostr);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var p = new PostUrlParameters
            {
                msg_signature = Request.Query["msg_signature"],
                timestamp = Request.Query["timestamp"],
                nonce = Request.Query["timestamp"],
            };

            try
            {
                var response = await _messageCenter.ProcessMessage(p, Request.Body).ConfigureAwait(false);
                return Content(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"消息处理异常: {ex.Message}, {ex.StackTrace}");
                return Content("");
            }
        }
    }
}