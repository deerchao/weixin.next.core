using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Weixin.Next.Json.Net;
using Weixin.Next.MP.Api;
using Weixin.Next.MP.Messaging;
using Weixin.Next.Sample.Models;

namespace Weixin.Next.Sample
{
    public static class MpServiceExtensions
    {
        public static IServiceCollection AddWeixinMp(this IServiceCollection services)
        {
            services.AddSingleton(p => p.GetRequiredService<IMpServiceFactory>().MessageCenter);
            services.AddSingleton(p => p.GetRequiredService<IMpServiceFactory>().JsapiTicketManager);
            services.AddSingleton(p => p.GetRequiredService<IMpServiceFactory>().AccessTokenManager);
            services.AddSingleton(p => p.GetRequiredService<IMpServiceFactory>().ApiConfig);
            return services;
        }
    }

    public interface IMpServiceFactory
    {
        MessageCenter MessageCenter { get; }
        IJsapiTicketManager JsapiTicketManager { get; }
        IAccessTokenManager AccessTokenManager { get; }
        ApiConfig ApiConfig { get; }
    }

    class MpServiceFactory : IMpServiceFactory
    {
        private readonly MpSettings _settings;

        public MpServiceFactory(MpSettings settings)
        {
            _settings = settings;
        }

        public MessageCenter MessageCenter { get; private set; }
        public IJsapiTicketManager JsapiTicketManager { get; private set; }
        public IAccessTokenManager AccessTokenManager { get; private set; }
        public ApiConfig ApiConfig { get; private set; }

        public void Build()
        {
            CreateMessageCenter();
            CreateApiConfig();
            CreateJsapiTicketManager();
        }


        private void CreateMessageCenter()
        {
            var appId = _settings.AppId;
            var token = _settings.Token;
            var encodingAESKey = _settings.EncodingAESKey;

            var messageCenter = string.IsNullOrEmpty(encodingAESKey)
                 ? new SampleMessageCenter()
                 : new SampleMessageCenter(appId, token, encodingAESKey);

            messageCenter.Initialize();

            MessageCenter = messageCenter;
        }


        private void CreateApiConfig()
        {
            var manager = CreateAccessTokenManager();
            var config = new ApiConfig
            {
                JsonParser = new JsonParser(),
                AccessTokenManager = manager,
                HttpClient = CreateHttpClient(),
            };
            manager.Config = config;

            AccessTokenManager = manager;
            ApiConfig = config;
            ApiHelper.SetDefaultConfig(config);
        }

        private AccessTokenManager CreateAccessTokenManager()
        {
            var appId = _settings.AppId;
            var appSecret = _settings.AppSecret;
            return new AccessTokenManager(appId, appSecret);
        }

        private HttpClient CreateHttpClient()
        {
            // 使用 LoggingHandler 为 HttpClient 添加日志功能

            var handler = new LoggingHandler(Log,
                () => _settings.LoggingEnabled,
                new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.Deflate |
                                             DecompressionMethods.GZip
                });
            return new HttpClient(handler);
        }

        private static void Log(string message)
        {
            // 可以改为写入到文件, 使用 NLog 等
            Debug.WriteLine(message);
        }

        private void CreateJsapiTicketManager()
        {
            JsapiTicketManager = new JsapiTicketManager(ApiConfig);
        }
    }
}