namespace Weixin.Next.Sample.Models
{
    public interface IMpSettings
    {
        string AppId { get; }
        string AppSecret { get; }
        string Token { get; }
        string EncodingAESKey { get; }
        bool LoggingEnabled { get; }
    }

    public class MpSettings : IMpSettings
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string Token { get; set; }
        public string EncodingAESKey { get; set; }

        public bool LoggingEnabled { get; set; }
    }
}
