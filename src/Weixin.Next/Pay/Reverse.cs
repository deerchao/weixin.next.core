using System.Collections.Generic;
using System.Xml.Linq;
using Weixin.Next.Common;

namespace Weixin.Next.Pay
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// 撤销订单
    /// </summary>
    public class Reverse : PayApi<Reverse.Outcoming, Reverse.Incoming, Reverse.ErrorCode>
    {
        public Reverse(Requester requester, bool checkSignature, bool sandbox, bool generateReport)
            : base(requester, checkSignature, sandbox, generateReport)
        {
        }

        protected override void GetApiUrl(Outcoming outcoming, out string interface_url, out bool requiresCert)
        {
            interface_url = ApiRootUrl + "secapi/pay/reverse";
            requiresCert = true;
        }

        protected override string GetReportOutTradeNo(Outcoming outcoming, Incoming incoming)
        {
            return outcoming.out_trade_no;
        }

        protected override string GetReportDeviceNo(Outcoming outcoming)
        {
            return null;
        }

        public class Outcoming : OutcomingData
        {
            /// <summary>
            /// 微信的订单号，优先使用 
            /// </summary>
            public string transaction_id { get; set; }
            /// <summary>
            /// 商户系统内部的订单号，当没提供transaction_id时需要传这个。 
            /// </summary>
            public string out_trade_no { get; set; }

            public override IEnumerable<KeyValuePair<string, string>> GetFields(IJsonParser jsonParser)
            {
                yield return new KeyValuePair<string, string>("transaction_id", transaction_id);
                yield return new KeyValuePair<string, string>("out_trade_no", out_trade_no);
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
            /// 是否需要继续调用撤销
            /// </summary>
            public bool recall { get; set; }

            protected override void DeserializeFields(List<KeyValuePair<string, string>> values, IJsonParser jsonParser, XElement xml)
            {
                appid = GetValue(values, "appid");
                mch_id = GetValue(values, "mch_id");
                sub_appid = GetValue(values, "sub_appid");
                sub_mch_id = GetValue(values, "sub_mch_id");
            }

            protected override void DeserializeSuccessFields(List<KeyValuePair<string, string>> values, IJsonParser jsonParser, XElement xml)
            {
                recall = GetValue(values, "device_info") == "Y";
            }
        }

        public enum ErrorCode
        {
            /// <summary>
            /// 接口返回错误
            /// 系统超时
            /// 请立即调用被扫订单结果查询API，查询当前订单状态，并根据订单的状态决定下一步的操作。
            /// </summary>
            SYSTEMERROR,
            /// <summary>
            /// 无效transaction_id
            /// 请求参数未按指引进行填写
            /// 参数错误，请重新检查
            /// </summary>
            INVALID_TRANSACTIONID,
            /// <summary>
            /// 参数错误
            /// 请求参数未按指引进行填写
            /// 请根据接口返回的详细信息检查您的程序
            /// </summary>
            PARAM_ERROR,
            /// <summary>
            /// 请使用post方法
            /// 未使用post传递参数
            /// 请检查请求参数是否通过post方法提交
            /// </summary>
            REQUIRE_POST_METHOD,
            /// <summary>
            /// 签名错误
            /// 参数签名结果不正确
            /// 请检查签名参数和方法是否都符合签名算法要求
            /// </summary>
            SIGNERROR,
            /// <summary>
            /// 订单无法撤销
            /// 订单有7天的撤销有效期，过期将不能撤销
            /// 请检查需要撤销的订单是否超过可撤销有效期
            /// </summary>
            REVERSE_EXPIRE,
            /// <summary>
            /// 无效请求
            /// 商户系统异常导致
            /// 请检查商户权限是否异常、重复请求支付、证书错误、频率限制等
            /// </summary>
            INVALID_REQUEST,
            /// <summary>
            /// 订单错误
            /// 业务错误导致交易失败
            /// 请检查用户账号是否异常、被风控、是否符合规则限制等
            /// </summary>
            TRADE_ERROR,
        }
    }
}
