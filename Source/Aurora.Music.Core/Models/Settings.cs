using Aurora.Shared.Helpers;
using System;
using Windows.UI.Xaml;

namespace Aurora.Music.Core.Models
{

    [Flags]
    public enum Effects
    {
        None = 0, Reverb = 2, Limiter = 4, Equalizer = 8, All = Reverb | Limiter | Equalizer
    }

    public enum Bitrate
    {
        _128, _192, _256, _320
    }

    public class Band
    {
        //
        // 摘要:
        //     获取或设置均衡器频段的增益。
        //
        // 返回结果:
        //     指示增益的值。
        public double Gain { get; set; }
        //
        // 摘要:
        //     获取或设置均衡器频段的频率中心。
        //
        // 返回结果:
        //     一个指示频率中心的值。
        public double FrequencyCenter { get; set; }
        //
        // 摘要:
        //     获取或设置均衡器频段的带宽。
        //
        // 返回结果:
        //     一个带宽值。
        public double Bandwidth { get; set; }
    }

    public class Settings
    {
        private static object lockable = new object();

        private const string SETTINGS_CONTAINER = "main";

        public bool IncludeMusicLibrary { get; set; } = true;
        public ElementTheme Theme { get; set; } = ElementTheme.Default;

        public bool WelcomeFinished { get; set; } = false;
        public string OutputDeviceID { get; set; }
        public double PlayerVolume { get; set; } = 50d;
        public Effects AudioGraphEffects { get; set; } = Effects.None;
        public string CategoryLastClicked { get; set; }

        public string LyricExtensionID { get; set; } = string.Empty;
        public string OnlineMusicExtensionID { get; set; } = string.Empty;
        public string MetaExtensionID { get; set; } = string.Empty;

        public Bitrate PreferredBitRate { get; set; } = Bitrate._256;
        public bool OnlinePurchase { get; set; } = false;
        public bool DebugModeEnabled { get; set; } = false;

        public string DownloadPathToken { get; set; } = string.Empty;
        
        public static Settings Load()
        {
            try
            {
                if (LocalSettingsHelper.GetContainer(SETTINGS_CONTAINER).ReadGroupSettings(out Settings s))
                {
                    return s;
                }
                else
                {
                    return new Settings();
                }
            }
            catch (Exception)
            {
                return new Settings();
            }
        }

        public void Save()
        {
            lock (lockable)
            {
                LocalSettingsHelper.GetContainer(SETTINGS_CONTAINER).WriteGroupSettings(this);
            }
        }

        public uint GetPreferredBitRate()
        {
            switch (PreferredBitRate)
            {
                case Bitrate._128:
                    return 128u;
                case Bitrate._192:
                    return 192u;
                case Bitrate._256:
                    return 256u;
                case Bitrate._320:
                    return 320u;
                default:
                    return 256u;
            }
        }
    }
}
