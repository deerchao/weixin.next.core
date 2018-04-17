using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weixin.Next.Common;
using Weixin.Next.Utilities;

namespace Weixin.Next.WXA
{
    /// <summary>
    /// 小程序登录会话
    /// </summary>
    public class LoginSession
    {
        public Guid Id { get; set; }
        public string Openid { get; set; }
        public string Unionid { get; set; }
        public string SessionKey { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public abstract class LoginSessionManagerBase : ILoginSessionManager
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


        protected abstract Task Save(LoginSession session);

        public abstract Task<LoginSession> Find(Guid id);

        public abstract Task<LoginSession> FindByOpenid(string openid);

        public abstract Task<LoginSession> FindByUnionid(string unionid);


        public async Task<LoginSession> Start(string code)
        {
            var result = await Session.Get(_appId, _appSecret, code, _config).ConfigureAwait(false);
            var session = new LoginSession
            {
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Openid = result.openid,
                Unionid = result.unionid,
                SessionKey = result.session_key,
            };
            await Save(session).ConfigureAwait(false);

            return session;
        }

        public async Task<bool> VerifySignature(Guid id, string rawData, string signature)
        {
            var session = await Find(id).ConfigureAwait(false);
            if (session == null)
                throw new LoginSessionNotFoundException();

            return Security.VerifySignature(rawData, session.SessionKey, signature);
        }

        public async Task<T> DecryptData<T>(Guid id, string encryptedData, string iv) where T : EncryptedData
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
    }

    /// <summary>
    /// 使用内存存储的小程序登录会话管理器
    /// </summary>
    public class LoginSessionManager : LoginSessionManagerBase
    {
        private static readonly Task<LoginSession> _null = Task.FromResult<LoginSession>(null);
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly Dictionary<Guid, LoginSession> _sessionsById = new Dictionary<Guid, LoginSession>();
        private readonly Dictionary<string, LoginSession> _sessionsByOpenid = new Dictionary<string, LoginSession>();
        private readonly Dictionary<string, LoginSession> _sessionsByUnionid = new Dictionary<string, LoginSession>();

        public LoginSessionManager(string appId, string appSecret, ApiConfig config)
            : base(appId, appSecret, config)
        {
        }


        public override Task<LoginSession> Find(Guid id)
        {
            _lock.EnterReadLock();

            LoginSession session;
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

        public override Task<LoginSession> FindByOpenid(string openid)
        {
            _lock.EnterReadLock();

            LoginSession session;
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

        public override Task<LoginSession> FindByUnionid(string unionid)
        {
            _lock.EnterReadLock();

            LoginSession session;
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

        protected override Task Save(LoginSession session)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_sessionsByOpenid.TryGetValue(session.Openid, out LoginSession oldSession))
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
