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
    public static class WeixinStartup
    {
        public static IServiceCollection AddWeixinMp(this IServiceCollection services, IMpSettings settings)
        {
            services.AddSingleton(settings);

            var builder = new MpBuilder(settings);
            builder.Build();

            services.AddSingleton<MessageCenter>(builder.MessageCenter);
            services.AddSingleton(builder.JsapiTicketManager);
            services.AddSingleton(builder.AccessTokenManager);
            services.AddSingleton(builder.ApiConfig);

            return services;
        }

        class MpBuilder
        {
            private readonly IMpSettings _settings;

            public MpBuilder(IMpSettings settings)
            {
                _settings = settings;
            }

            public SampleMessageCenter MessageCenter { get; private set; }
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
}