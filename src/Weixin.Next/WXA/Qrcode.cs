using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Weixin.Next.Common;

namespace Weixin.Next.WXA
{
    /// <summary>
    /// 微信小程序二维码
    /// </summary>
    public static class Qrcode
    {
        /// <summary>
        /// 获取小程序码，适用于需要的码数量极多的业务场景。通过该接口生成的小程序码，永久有效，数量暂无限制。
        /// </summary>
        /// <param name="scene">最大32个可见字符，只支持数字，大小写英文以及部分特殊字符：!#$&amp;'()*+,/:;=?@-._~，其它字符请自行编码为合法字符</param>
        /// <param name="page">主页，可选。必须是已经发布的小程序存在的页面（否则报错），例如 pages/index/index, 根路径前不要填加 /,不能携带参数（参数请放在scene字段里），如果不填写这个字段，默认跳主页面</param>
        /// <param name="width">二维码的宽度，单位 px，最小 280px，最大 1280px，默认 430px</param>
        /// <param name="autoColor">是否自动配置线条颜色，如果颜色依然是黑色，则说明不建议配置主色调，默认 false</param>
        /// <param name="lineColor">autoColor 为 false 时生效</param>
        /// <param name="isHyaline">是否需要透明底色，为 true 时，生成透明底色的小程序</param>
        /// <param name="config"></param>
        /// <returns>图片二进制内容</returns>
        public static Task<Stream> GetUnlimited(string scene, string page = null, int width = 430, bool autoColor = false, Color? lineColor = null, bool isHyaline = false, ApiConfig config = null)
        {
            return ApiHelper.PostStream($"https://api.weixin.qq.com/wxa/getwxacodeunlimit?$acac$", new
            {
                scene,
                page,
                width,
                auto_color = autoColor,
                line_color = lineColor == null
                    ? null
                    : new
                    {
                        r = (int)lineColor.Value.R,
                        g = (int)lineColor.Value.G,
                        b = (int)lineColor.Value.B,
                    },
                is_hyaline = isHyaline,
            }, config);
        }

        /// <summary>
        /// 获取小程序码，适用于需要的码数量较少的业务场景。通过该接口生成的小程序码，永久有效，有数量限制。
        /// </summary>
        /// <param name="path">扫码进入的小程序页面路径，最大长度 128 字节，不能为空；对于小游戏，可以只传入 query 部分，来实现传参效果，如：传入 "?foo=bar"，即可在 wx.getLaunchOptionsSync 接口中的 query 参数获取到 {foo:"bar"}。</param>
        /// <param name="width">二维码的宽度，单位 px，最小 280px，最大 1280px，默认 430px</param>
        /// <param name="autoColor">是否自动配置线条颜色，如果颜色依然是黑色，则说明不建议配置主色调，默认 false</param>
        /// <param name="lineColor">autoColor 为 false 时生效</param>
        /// <param name="isHyaline">是否需要透明底色，为 true 时，生成透明底色的小程序</param>
        /// <param name="config"></param>
        /// <returns>图片二进制内容</returns>
        public static Task<Stream> Get(string path, int width = 430, bool autoColor = false, Color? lineColor = null, bool isHyaline = false, ApiConfig config = null)
        {
            return ApiHelper.PostStream($"https://api.weixin.qq.com/wxa/getwxacode?$acac$", new
            {
                path,
                width,
                auto_color = autoColor,
                line_color = lineColor == null
                    ? null
                    : new
                    {
                        r = (int)lineColor.Value.R,
                        g = (int)lineColor.Value.G,
                        b = (int)lineColor.Value.B,
                    },
                is_hyaline = isHyaline,
            }, config);
        }
    }
}
