namespace Weixin.Next.WXA
{
    public class EncryptedData
    {
        public Watermark watermark { get; set; }
    }

    public class Watermark
    {
        public string appid { get; set; }
        public long timestamp { get; set; }
    }

    public class GetUserInfoResult : EncryptedData
    {
        public string openId { get; set; }
        public string nickName { get; set; }
        public int gender { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string avatarUrl { get; set; }
        public string unionId { get; set; }
    }

    public class GetPhoneNumberResult : EncryptedData
    {
        public string phoneNumber { get; set; }
        public string purePhoneNumber { get; set; }
        public string countryCode { get; set; }
    }
}
