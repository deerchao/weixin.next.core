using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weixin.Next.Common;
using Weixin.Next.Utilities;

namespace Weixin.Next.WXA
{
    public abstract class LoginSessionManagerBase<TSession, TKey> : ILoginSessionManager<TSession, TKey>
        where TSession : LoginSession<TKey>
        where TKey : struct
    {
        private readonly string _appId;
        private readonly string _appSecret;
        private readonly ApiConfig _config;


        protected LoginSessionManagerBase(string appId, string appSecret, ApiConfig config)
        {
            _appId = appId;
            _appSecret = appSecret;
            _config = config;
        }


        public TimeSpan AllowedTimestampDiff { get; set; } = TimeSpan.FromMinutes(5);


        protected abstract Task Save(TSession session);

        public abstract Task<TSession> Find(TKey id);

        public abstract Task<TSession> FindByOpenid(string openid);

        public abstract Task<TSession> FindByUnionid(string unionid);


        public async Task<TSession> Start(string code)
        {
            var result = await Session.Get(_appId, _appSecret, code, _config).ConfigureAwait(false);

            var session = CreateSession(result.openid, result.unionid, result.session_key, DateTime.Now);

            await Save(session).ConfigureAwait(false);

            return session;
        }

        public async Task<bool> VerifySignature(TKey id, string rawData, string signature)
        {
            var session = await Find(id).ConfigureAwait(false);
            if (session == null)
                throw new LoginSessionNotFoundException();

            return Security.VerifySignature(rawData, session.SessionKey, signature);
        }

        public async Task<T> DecryptData<T>(TKey id, string encryptedData, string iv) where T : EncryptedData
        {
            var session = await Find(id).ConfigureAwait(false);
            if (session == null)
                throw new LoginSessionNotFoundException();

            var decrypted = Security.DecryptData(encryptedData, session.SessionKey, iv);
            if (decrypted == null)
                throw new DataDecryptException();

            var parser = _config?.JsonParser ?? ApiHelper.DefaultConfig?.JsonParser;
            var value = parser.Parse(decrypted);
            var result = parser.Build<T>(value);

            if (result.watermark != null &&
                result.watermark.appid == _appId &&
                (result.watermark.timestamp.FromWeixinTimestamp() - DateTime.Now).Duration() <= AllowedTimestampDiff.Duration())
                return result;

            throw new InvalidWartermakException();
        }

        /// <summary>
        /// 生成新的会话。
        /// </summary>
        /// <returns></returns>
        protected abstract TSession CreateSession(string openid, string unionid, string sessionKey, DateTime createTime);
    }

    /// <summary>
    /// 使用内存存储的小程序登录会话管理器
    /// </summary>
    public abstract class LoginSessionManager<TSession, TKey> : LoginSessionManagerBase<TSession, TKey>
        where TSession : LoginSession<TKey>
        where TKey : struct
    {
        private static readonly Task<TSession> _null = Task.FromResult<TSession>(null);
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly Dictionary<TKey, TSession> _sessionsById = new Dictionary<TKey, TSession>();
        private readonly Dictionary<string, TSession> _sessionsByOpenid = new Dictionary<string, TSession>();
        private readonly Dictionary<string, TSession> _sessionsByUnionid = new Dictionary<string, TSession>();

        public LoginSessionManager(string appId, string appSecret, ApiConfig config)
            : base(appId, appSecret, config)
        {
        }


        public override Task<TSession> Find(TKey id)
        {
            _lock.EnterReadLock();

            TSession session;
            try
            {
                _sessionsById.TryGetValue(id, out session);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return session == null
                ? _null
                : Task.FromResult(session);
        }

        public override Task<TSession> FindByOpenid(string openid)
        {
            _lock.EnterReadLock();

            TSession session;
            try
            {
                _sessionsByOpenid.TryGetValue(openid, out session);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return session == null
                ? _null
                : Task.FromResult(session);
        }

        public override Task<TSession> FindByUnionid(string unionid)
        {
            _lock.EnterReadLock();

            TSession session;
            try
            {
                _sessionsByUnionid.TryGetValue(unionid, out session);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return session == null
                ? _null
                : Task.FromResult(session);
        }

        protected override Task Save(TSession session)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_sessionsByOpenid.TryGetValue(session.Openid, out TSession oldSession))
                {
                    _sessionsByOpenid.Remove(oldSession.Openid);
                    _sessionsById.Remove(oldSession.Id);
                    if (!string.IsNullOrEmpty(oldSession.Unionid))
                        _sessionsByUnionid.Remove(oldSession.Unionid);
                }

                _sessionsById.Add(session.Id, session);
                _sessionsByOpenid.Add(session.Openid, session);
                if (!string.IsNullOrEmpty(session.Unionid))
                    _sessionsByUnionid.Add(session.Unionid, session);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            return Task.CompletedTask;
        }
    }
}
