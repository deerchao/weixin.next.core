using System.Collections.Generic;
using System.Xml.Linq;
using Weixin.Next.Common;

namespace Weixin.Next.Pay
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// 转换短链接
    /// </summary>
    public class ShortUrl : PayApi<ShortUrl.Outcoming, ShortUrl.Incoming, ShortUrl.ErrorCode>
    {
        public ShortUrl(Requester requester, bool checkSignature, bool sandbox, bool generateReport) 
            : base(requester, checkSignature, sandbox, generateReport)
        {
        }

        protected override void GetApiUrl(Outcoming outcoming, out string interface_url, out bool requiresCert)
        {
            interface_url = Stage.ProductionRootUrl + "tools/shorturl";
            requiresCert = false;
        }

        protected override string GetReportDeviceNo(Outcoming outcoming)
        {
            return null;
        }

        protected override string GetReportOutTradeNo(Outcoming outcoming, Incoming incoming)
        {
            return null;
        }

        public class Outcoming : OutcomingData
        {
            /// <summary>
            /// 需要转换的URL，签名用原串，传输需URLencode
            /// </summary>
            public string long_url { get; set; }

            public override IEnumerable<KeyValuePair<string, string>> GetFields(IJsonParser jsonParser)
            {
                yield return new KeyValuePair<string, string>("long_url", long_url);
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
            /// 转换后的URL
            /// </summary>
            public string short_url { get; set; }


            protected override void DeserializeFields(List<KeyValuePair<string, string>> values, IJsonParser jsonParser, XElement xml)
            {
                appid = GetValue(values, "appid");
                mch_id = GetValue(values, "mch_id");
                sub_appid = GetValue(values, "sub_appid");
                sub_mch_id = GetValue(values, "sub_mch_id");
            }

            protected override void DeserializeSuccessFields(List<KeyValuePair<string, string>> values, IJsonParser jsonParser, XElement xml)
            {
                short_url = GetValue(values, "short_url");
            }
        }

        public enum ErrorCode
        {
            /// <summary>
            /// 签名错误
            /// 参数签名结果不正确
            /// 请检查签名参数和方法是否都符合签名算法要求
            /// </summary>
            SIGNERROR,
            /// <summary>
            /// 请使用post方法
            /// 未使用post传递参数
            /// 请检查请求参数是否通过post方法提交
            /// </summary>
            REQUIRE_POST_METHOD,
            /// <summary>
            /// APPID不存在
            /// 参数中缺少APPID
            /// 请检查APPID是否正确
            /// </summary>
            APPID_NOT_EXIST,
            /// <summary>
            /// MCHID不存在
            /// 参数中缺少MCHID
            /// 请检查MCHID是否正确
            /// </summary>
            MCHID_NOT_EXIST,
            /// <summary>
            /// appid和mch_id不匹配
            /// appid和mch_id不匹配
            /// 请确认appid和mch_id是否匹配
            /// </summary>
            APPID_MCHID_NOT_MATCH,
            /// <summary>
            /// 缺少参数
            /// 缺少必要的请求参数
            /// 请检查参数是否齐全
            /// </summary>
            LACK_PARAMS,
            /// <summary>
            /// XML格式错误
            /// XML格式错误
            /// 请检查XML参数格式是否正确
            /// </summary>
            XML_FORMAT_ERROR,
            /// <summary>
            /// post数据为空
            /// post数据不能为空
            /// 请检查post数据是否为空
            /// </summary>
            POST_DATA_EMPTY,
        }
    }
}
