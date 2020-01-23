using System;
using System.Threading.Tasks;
using Weixin.Next.Common;

namespace Weixin.Next.WXA
{

    public class LoginSessionNotFoundException : Exception
    {
        public LoginSessionNotFoundException()
            : base("未找到指定的登录会话")
        {
        }
    }

    public class DataDecryptException : Exception
    {
        public DataDecryptException()
            : base("数据解密失败")
        {
        }
    }

    public class InvalidWartermakException : Exception
    {
        public InvalidWartermakException()
            : base("非法的数据水印")
        {
        }
    }

    /// <summary>
    /// 小程序登录会话。<typeparamref name="TKey" />为主键类型，如 long 或 Guid
    /// </summary>
    public class LoginSession<TKey>
        where TKey : struct
    {
        public TKey Id { get; set; }
        public string Openid { get; set; }
        public string Unionid { get; set; }
        public string SessionKey { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// <typeparamref name="TKey" />为主键类型，如 long 或 Guid
    /// </summary>
    public interface ILoginSessionManager<TSession, TKey>
        where TSession: LoginSession<TKey>
        where TKey: struct
    {
        /// <summary>
        /// <para>根据客户端调用 wx.login 的结果，开始一段登录会话</para>
        /// <para>同一 openid 的会话，如已存在将会被替换</para>
        /// </summary>
        /// <param name="code">调用接口 wx.login 获取的临时登录凭证</param>
        /// <returns>刚刚开始的登录会话</returns>
        /// <exception cref="ApiException">使用 code 换取 session 信息时失败</exception>
        Task<TSession> Start(string code);
        /// <summary>
        /// 根据会话 id 查找会话对象
        /// </summary>
        /// <param name="id">会话 id</param>
        /// <returns>null, 如果未找到对象; 否则是找到的会话对象</returns>
        Task<TSession> Find(TKey id);
        /// <summary>
        /// 根据会话 openid 查找会话对象
        /// </summary>
        /// <param name="openid">用户 openid</param>
        /// <returns>null, 如果未找到对象; 否则是找到的会话对象</returns>
        Task<TSession> FindByOpenid(string openid);
        /// <summary>
        /// 根据会话 unionid 查找会话对象
        /// </summary>
        /// <param name="unionid">用户 unionid</param>
        /// <returns>null, 如果未找到对象; 否则是找到的会话对象</returns>
        Task<TSession> FindByUnionid(string unionid);
        /// <summary>
        /// 验证微信客户端接口返回数据的签名是否正确
        /// </summary>
        /// <param name="id">会话 id</param>
        /// <param name="rawData">微信返回的 rawData</param>
        /// <param name="signature">微信返回的 signature</param>
        /// <returns>true, 如果签名正确；否则 false</returns>
        /// <exception cref="LoginSessionNotFoundException">未找到指定的登录会话</exception>
        Task<bool> VerifySignature(TKey id, string rawData, string signature);
        /// <summary>
        /// 对微信客户端返回的加密数据进行解密并构造数据对象
        /// </summary>
        /// <param name="id">会话 id</param>
        /// <param name="encryptedData">微信返回的加密信息 encryptedData</param>
        /// <param name="iv">微信返回的加密向量 iv</param>
        /// <returns>由解密后的 JSON 字符串构造的数据对象</returns>
        /// <exception cref="LoginSessionNotFoundException">未找到指定的登录会话</exception>
        /// <exception cref="DataDecryptException">数据解密失败</exception>
        /// <exception cref="InvalidWartermakException">非法的数据水印</exception>
        Task<T> DecryptData<T>(TKey id, string encryptedData, string iv)
            where T : EncryptedData;
    }
}
