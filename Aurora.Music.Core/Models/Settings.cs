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

    public class Settings
    {
        private static object lockable = new object();

        private const string SETTINGS_CONTAINER = "main";

        public bool IncludeMusicLibrary { get; set; } = true;
        public ElementTheme Theme { get; set; } = ElementTheme.Default;

        public bool WelcomeFinished { get; set; } = false;
        public string OutputDeviceID { get; set; }
        private string lyricSource;
        public int LyricSource { get; set; } = 0;
        public double PlayerVolume { get; set; } = 50d;
        public Effects AudioGraphEffects { get; internal set; } = Effects.None;

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
    }
}
