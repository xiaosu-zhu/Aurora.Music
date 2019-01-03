// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;

using Newtonsoft.Json;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class DoubanLogin : ContentDialog
    {
        static readonly Regex test = new Regex(@"(^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))"
                +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$)"
                + @"|(^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$)");

        public DoubanLogin()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var def = args.GetDeferral();
            if (await Login(Account.Text, Password.Password))
            {

            }
            else
            {
                args.Cancel = true;
            }
            def.Complete();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        public async Task<bool> Login(string username, string password)
        {
            if (!Settings.Current.VerifyDoubanLogin())
            {
                Settings.Current.DoubanToken = null;
                Settings.Current.DoubanUserName = null;
                Settings.Current.DoubanUserID = null;
                // Login and get access_token
                var dix = new Dictionary<string, string>
                {
                    ["apikey"] = "02646d3fb69a52ff072d47bf23cef8fd",
                    ["client_id"] = "02646d3fb69a52ff072d47bf23cef8fd",
                    ["client_secret"] = "cde5d61429abcd7c",
                    ["udid"] = "b88146214e19b8a8244c9bc0e2789da68955234d",
                    ["douban_udid"] = "b635779c65b816b13b330b68921c0f8edc049590",
                    ["device_id"] = "b88146214e19b8a8244c9bc0e2789da68955234d",
                    ["grant_type"] = "password",
                    ["redirect_uri"] = "http://www.douban.com/mobile/fm",
                    ["username"] = username,
                    ["password"] = password
                };

                var json = await ApiRequestHelper.HttpPostForm("https://www.douban.com/service/auth2/token", dix, true);

                if (json.IsNullorEmpty())
                {
                    FailText.Visibility = Visibility.Visible;
                    FailText.Text = Consts.Localizer.GetString("UnknownError");
                    return false;
                }

                var result = JsonConvert.DeserializeAnonymousType(json, new { msg = "", access_token = "", request = "", douban_user_name = "", douban_user_id = "", refresh_token = "", code = 0, expires_in = 0u, });

                if ((result.msg as string).IsNullorEmpty() && !result.access_token.IsNullorEmpty())
                {
                    FailText.Visibility = Visibility.Collapsed;
                    Settings.Current.DoubanLogin = DateTime.Now;
                    Settings.Current.DoubanExpireTime = result.expires_in;
                    Settings.Current.DoubanToken = result.access_token;
                    Settings.Current.DoubanUserName = result.douban_user_name;
                    Settings.Current.DoubanUserID = result.douban_user_id;
                    Settings.Current.Save();
                    return true;
                }
                else
                {
                    FailText.Visibility = Visibility.Visible;
                    FailText.Text = $"{result.msg} ({result.code})";
                    return false;
                }

                ///{"msg":"username_password_mismatch","code":120,"request":"POST \/auth2\/token"}
                ///
                ///{"access_token":"4b6c984fbd04c351e245c8ef370e49f3",
                ///"douban_user_name":"EmbraceZ","douban_user_id":"172711138",
                ///"expires_in":7775999,
                ///"refresh_token":"7772cc7a999b3b146658656fccd1b13c"}
            }
            else
            {
                // do nothing
                return true;
            }
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password.Header = Password.Password.Length >= 8 ? Consts.Localizer.GetString("Password") : Consts.Localizer.GetString("Invalid");
            IsPrimaryButtonEnabled = Password.Password.Length >= 8 && test.IsMatch(Account.Text);
        }

        private void Account_TextChanged(object sender, TextChangedEventArgs e)
        {
            var b = test.IsMatch(Account.Text);
            Account.Header = b ? Consts.Localizer.GetString("Account") : Consts.Localizer.GetString("Invalid");
            IsPrimaryButtonEnabled = Password.Password.Length >= 8 && b;
        }
    }
}
