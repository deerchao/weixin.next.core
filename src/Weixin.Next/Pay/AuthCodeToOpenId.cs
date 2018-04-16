using System.Collections.Generic;
using System.Xml.Linq;
using Weixin.Next.Common;

namespace Weixin.Next.Pay
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// 授权码查询OPENID接口
    /// </summary>
    public class AuthCodeToOpenId : PayApi<AuthCodeToOpenId.Outcoming, AuthCodeToOpenId.Incoming, AuthCodeToOpenId.ErrorCode>
    {
        public AuthCodeToOpenId(Requester requester, bool checkSignature, bool sandbox, bool generateReport)
            : base(requester, checkSignature, sandbox, generateReport)
        {
        }

        protected override void GetApiUrl(Outcoming outcoming, out string interface_url, out bool requiresCert)
        {
            interface_url = Stage.ProductionRootUrl + "tools/authcodetoopenid";
            requiresCert = false;
        }

        protected override string GetReportOutTradeNo(Outcoming outcoming, Incoming incoming)
        {
            return null;
        }

        protected override string GetReportDeviceNo(Outcoming outcoming)
        {
            return null;
        }

        public class Outcoming : OutcomingData
        {
            /// <summary>
            /// 扫码支付授权码，设备读取用户微信中的条码或者二维码信息
            /// </summary>
            public string auth_code { get; set; }

            public override IEnumerable<KeyValuePair<string, string>> GetFields(IJsonParser jsonParser)
            {
                yield return new KeyValuePair<string, string>("auth_code", auth_code);
            }
        }

        public class Incoming : IncomingData<ErrorCode>
        {
            /// <summary>
            /// 调用接口提交的公众账号ID, 仅在return_code为SUCCESS的时候有意义
            /// </summary>
            public string appid { get; set; }
            /// <summary>
            /// 调用接口提交的商户号, 仅在return_code为SUCCESS的时候有意义
            /// </summary>
            public string mch_id { get; set; }
            /// <summary>
            /// 微信分配的子商户公众账号ID, 仅在服务商账号调用且return_code为SUCCESS的时候有意义
            /// </summary>
            public string sub_appid { get; set; }
            /// <summary>
            /// 微信支付分配的子商户号, 仅在服务商账号调用且return_code为SUCCESS的时候有意义
            /// </summary>
            public string sub_mch_id { get; set; }
            /// <summary>
            /// 用户在商户appid下的唯一标识, 在return_code 和result_code都为SUCCESS的时候有意义
            /// </summary>
            public string openid { get; set; }
            /// <summary>
            /// 用户在子商户appid下的唯一标识, 仅在服务商账号调用且return_code 和result_code都为SUCCESS的时候有意义
            /// </summary>
            public string sub_openid { get; set; }

            protected override void DeserializeFields(List<KeyValuePair<string, string>> values, IJsonParser jsonParser, XElement xml)
            {
                appid = GetValue(values, "appid");
                mch_id = GetValue(values, "mch_id");
                sub_appid = GetValue(values, "sub_appid");
                sub_mch_id = GetValue(values, "sub_mch_id");
            }

            protected override void DeserializeSuccessFields(List<KeyValuePair<string, string>> values, IJsonParser jsonParser, XElement xml)
            {
                openid = GetValue(values, "openid");
                sub_openid = GetValue(values, "sub_openid");
            }
        }

        public enum ErrorCode
        {
            /// <summary>
            /// 系统错误
            /// </summary>
            SYSTEMERROR,
            /// <summary>
            /// 授权码过期
            /// </summary>
            AUTHCODEEXPIRE,
            /// <summary>
            /// 授权码错误 
            /// </summary>
            AUTH_CODE_ERROR,
            /// <summary>
            /// 授权码检验错误
            /// </summary>
            AUTH_CODE_INVALID
        }
    }
}
