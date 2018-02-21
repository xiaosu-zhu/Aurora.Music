// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.Storage;
using Windows.UI;

namespace Aurora.Shared.Helpers
{

    public static class RoamingSettingsHelper
    {
        /// <summary>
        /// Read a RoamingSetting's Value and clear it
        /// </summary>
        /// <param name="key">Setting's Key</param>
        /// <returns>Setting's Value</returns>
        public static object ReadResetSettingsValue(string key)
        {
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey(key))
            {
                var value = ApplicationData.Current.RoamingSettings.Values[key];
                ApplicationData.Current.RoamingSettings.Values.Remove(key);
                return value;
            }
            return null;
        }

        /// <summary>
        /// Read a RoamingSetting's Value
        /// </summary>
        /// <param name="key">Setting's Key</param>
        /// <returns>Setting's Value</returns>
        public static object ReadSettingsValue(string key)
        {
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey(key))
            {
                return ApplicationData.Current.RoamingSettings.Values[key];
            }
            return null;
        }

        /// <summary>
        /// Clear all Roaming settings. USE IT AT YOUR OWN RISK!
        /// </summary>
        /// <returns>Success or not</returns>
        public static bool ClearAllSettings()
        {
            try
            {
                ApplicationData.Current.RoamingSettings.Values.Clear();
                foreach (var container in ApplicationData.Current.RoamingSettings.Containers)
                {
                    ApplicationData.Current.RoamingSettings.DeleteContainer(container.Key);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Save/Overwrite a RoamingSetting
        /// </summary>
        /// <param name="key">Setting's Key</param>
        /// <param name="value">Setting's Value</param>
        /// <returns></returns>
        public static bool WriteSettingsValue(string key, object value)
        {
            try
            {
                var container = ApplicationData.Current.RoamingSettings;
                SettingsHelper.DirectWrite(key, value, container);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static ApplicationDataContainer GetContainer(string key)
        {
            return ApplicationData.Current.
                RoamingSettings.CreateContainer(key, ApplicationDataCreateDisposition.Always);
        }
    }

    public static class LocalSettingsHelper
    {

        /// <summary>
        /// Read a LocalSetting's Value and clear it
        /// </summary>
        /// <param name="key">Setting's Key</param>
        /// <returns>Setting's Value</returns>
        public static object ReadResetSettingsValue(string key)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                var value = ApplicationData.Current.LocalSettings.Values[key];
                ApplicationData.Current.LocalSettings.Values.Remove(key);
                return value;
            }

            return null;
        }

        /// <summary>
        /// Read a LocalSetting's Value
        /// </summary>
        /// <param name="key">Setting's Key</param>
        /// <returns>Setting's Value</returns>
        public static object ReadSettingsValue(string key)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                return ApplicationData.Current.LocalSettings.Values[key];
            }
            return null;
        }

        /// <summary>
        /// Clear all Local settings. USE IT AT YOUR OWN RISK!
        /// </summary>
        /// <returns>Success or not</returns>
        public static bool ClearAllSettings()
        {
            try
            {
                ApplicationData.Current.LocalSettings.Values.Clear();
                foreach (var container in ApplicationData.Current.LocalSettings.Containers)
                {
                    ApplicationData.Current.LocalSettings.DeleteContainer(container.Key);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Save/Overwrite a LocalSetting
        /// </summary>
        /// <param name="key">Setting's Key</param>
        /// <param name="value">Setting's Value</param>
        /// <returns></returns>
        public static bool WriteSettingsValue(string key, object value)
        {
            try
            {
                var container = ApplicationData.Current.LocalSettings;
                SettingsHelper.DirectWrite(key, value, container);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static ApplicationDataContainer GetContainer(string key)
        {
            return ApplicationData.Current.
                LocalSettings.CreateContainer(key, ApplicationDataCreateDisposition.Always);
        }
    }
    public static class SettingsHelper
    {
        /// <summary>
        /// Read a LocalSetting's Value and clear it
        /// </summary>
        /// <param name="key">Setting's Key</param>
        /// <returns>Setting's Value</returns>
        public static object ReadResetSettingsValue(this ApplicationDataContainer container, string key)
        {
            if (container.Values.ContainsKey(key))
            {
                var value = container.Values[key];
                container.Values.Remove(key);
                return value;
            }

            return null;
        }

        /// <summary>
        /// Read a LocalSetting's Value
        /// </summary>
        /// <param name="key">Setting's Key</param>
        /// <returns>Setting's Value</returns>
        public static object ReadSettingsValue(this ApplicationDataContainer container, string key)
        {
            if (container.Values.ContainsKey(key))
            {
                return container.Values[key];
            }
            return null;
        }

        /// <summary>
        /// Save/Overwrite a LocalSetting
        /// </summary>
        /// <param name="key">Setting's Key</param>
        /// <param name="value">Setting's Value</param>
        /// <returns></returns>
        public static void WriteSettingsValue(this ApplicationDataContainer container, string key, object value)
        {
            if (value is DateTime)
            {
                container.Values[key] = ((DateTime)value).ToBinary();
            }
            else if (value is Enum)
            {
                container.Values[key] = ((Enum)value).ToString();
            }
            else
            {
                container.Values[key] = value;
            }
        }

        public static ApplicationDataContainer GetContainer(this ApplicationDataContainer container, string key)
        {
            return container.CreateContainer(key, ApplicationDataCreateDisposition.Always);
        }

        public static object DirectRead(string key, ApplicationDataContainer subContainer)
        {
            return subContainer.Values[key];
        }

        /// <summary>
        /// 如果值是 DateTime, 作处理（work around）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="container"></param>
        public static void DirectWrite(string key, object value, ApplicationDataContainer container)
        {
            if (value is DateTime)
            {
                container.Values[key] = ((DateTime)value).ToBinary();
            }
            else if (value is Enum)
            {
                container.Values[key] = ((Enum)value).ToString();
            }
            else if (value is Color)
            {
                container.Values[key] = ((Color)value).A + ":|:" + ((Color)value).R + ":|:" + ((Color)value).G + ":|:" + ((Color)value).B;
            }
            else
            {
                container.Values[key] = value;
            }
        }

        public static bool ReadGroupSettings<T>(this ApplicationDataContainer mainContainer, out T source) where T : new()
        {
            var type = typeof(T);
            var obj = Activator.CreateInstance(type);
            foreach (var member in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                try
                {
                    if (member.PropertyType.IsArray)
                    {
                        var subContainer = mainContainer.CreateContainer(member.Name, ApplicationDataCreateDisposition.Always);
                        var res = ReadArraySettings(subContainer);
                        if (res == null || res.Length == 0)
                        {

                        }
                        else
                        {
                            if (member.PropertyType == typeof(DateTime[]))
                            {
                                List<DateTime> times = new List<DateTime>();
                                foreach (var time in res)
                                {
                                    times.Add(DateTime.FromBinary((long)time));
                                }
                                member.SetValue(obj, times.ToArray());
                            }
                            else
                            {
                                member.SetValue(obj, res);
                            }
                        }
                    }
                    else if (member.PropertyType == typeof(DateTime))
                    {
                        var l = (long?)DirectRead(member.Name, mainContainer);
                        if (l != null && l != default(long))
                        {
                            member.SetValue(obj, DateTime.FromBinary((long)l));
                        }

                    }
                    else if (member.PropertyType == typeof(Color))
                    {
                        var s = (string)DirectRead(member.Name, mainContainer);
                        if (!s.IsNullorEmpty())
                        {
                            var sarray = s.Split(new string[] { ":|:" }, StringSplitOptions.RemoveEmptyEntries);
                            member.SetValue(obj, Color.FromArgb(byte.Parse(sarray[0]), byte.Parse(sarray[1]), byte.Parse(sarray[2]), byte.Parse(sarray[3])));
                        }
                    }
                    // Holy shit! WinRT's type is really different from the legacy type.
                    else if (member.PropertyType.GetTypeInfo().IsEnum)
                    {
                        var s = (string)DirectRead(member.Name, mainContainer);
                        if (s != null)
                        {
                            member.SetValue(obj, Enum.Parse(member.PropertyType, s));
                        }
                    }
                    else
                    {
                        var v = DirectRead(member.Name, mainContainer);
                        if (v != null)
                        {
                            member.SetValue(obj, v);
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            source = (T)obj;
            return true;
        }

        /// <summary>
        /// 读取数组数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subContainer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Array ReadArraySettings(ApplicationDataContainer subContainer)
        {
            try
            {
                if (subContainer.Values.ContainsKey("Count"))
                {
                    int i = (int)subContainer.Values["Count"];
                    var list = Array.CreateInstance(DirectRead("0", subContainer).GetType(), i);
                    for (int j = 0; j < i; j++)
                    {
                        list.SetValue(DirectRead(j.ToString(), subContainer), j);
                    }
                    return list;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 写入数组值
        /// </summary>
        /// <param name="subContainer"></param>
        /// <param name="value"></param>
        public static void WriteArraySettings(ApplicationDataContainer subContainer, Array value)
        {
            int i = 0;
            if (value.IsNullorEmpty())
            {
                subContainer.Values["Count"] = 0;
                return;
            }
            foreach (var item in value)
            {
                DirectWrite(i.ToString(), item, subContainer);
                i++;
            }
            subContainer.Values["Count"] = i;
        }

        public static void WriteGroupSettings<T>(this ApplicationDataContainer mainContainer, T source) where T : new()
        {
            var type = typeof(T);
            foreach (var member in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var value = member.GetValue(source);
                if (value is Array)
                {
                    var subContainer = mainContainer.CreateContainer(member.Name, ApplicationDataCreateDisposition.Always);
                    WriteArraySettings(subContainer, value as Array);
                }
                else
                {
                    DirectWrite(member.Name, value, mainContainer);
                }
            }
        }
    }
}
