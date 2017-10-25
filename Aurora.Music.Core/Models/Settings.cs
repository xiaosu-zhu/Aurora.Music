using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace Aurora.Music.Core.Models
{
    public class Settings
    {
        private static object lockable = new object();

        private const string SETTINGS_CONTAINER = "main";

        public bool IncludeMusicLibrary { get; set; } = true;
        public ElementTheme Theme { get; set; } = ElementTheme.Default;

        public string[] FolderPaths { get; set; }

        public void AddFolderPath(string path)
        {
            var list = new List<string>(FolderPaths)
            {
                path
            };
            FolderPaths = list.ToArray();
            Save();
        }

        public static Settings Load()
        {
            try
            {
                if(LocalSettingsHelper.GetContainer(SETTINGS_CONTAINER).ReadGroupSettings(out Settings s))
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
