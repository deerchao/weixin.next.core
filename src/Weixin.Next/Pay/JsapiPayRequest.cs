using System;
using System.Collections.Generic;
using Weixin.Next.Utilities;

namespace Weixin.Next.Pay
{
    public class JsapiPayRequest
    {
        public string appId { get; set; }
        public string timeStamp { get; set; }
        public string nonceStr { get; set; }
        public string package { get; set; }
        public string signType { get; set; }
        public string paySign { get; set; }

        internal static JsapiPayRequest Create(string appId, string prepayId)
        {
            return new JsapiPayRequest
            {
                appId = appId,
                timeStamp = DateTime.Now.ToWeixinTimestamp().ToString("d"),
                nonceStr = Guid.NewGuid().ToString("n"),
                package = "prepay_id=" + prepayId,
                signType = "MD5",
            };
        }

        internal JsapiPayRequest PouplateSign(Requester requester)
        {
            paySign = requester.ComputeSign(new[]
            {
                new KeyValuePair<string, string>("appId", this.appId),
                new KeyValuePair<string, string>("timeStamp", this.timeStamp),
                new KeyValuePair<string, string>("nonceStr", this.nonceStr),
                new KeyValuePair<string, string>("package", this.package),
                new KeyValuePair<string, string>("signType", this.signType),
            });

            return this;
        }
    }
}
