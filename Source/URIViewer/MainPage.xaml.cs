using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace URIViewer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var uri = new Uri((sender as TextBox).Text);

                Main.Text = $"AbsolutePath: {uri.AbsolutePath}{Environment.NewLine}";

                Main.Text += $"AbsoluteUri: {uri.AbsoluteUri}{Environment.NewLine}";

                Main.Text += $"Authority: {uri.Authority}{Environment.NewLine}";

                Main.Text += $"DnsSafeHost: {uri.DnsSafeHost}{Environment.NewLine}";

                Main.Text += $"Fragment: {uri.Fragment}{Environment.NewLine}";

                Main.Text += $"Host: {uri.Host}{Environment.NewLine}";

                Main.Text += $"HostNameType: {uri.HostNameType}{Environment.NewLine}";

                Main.Text += $"IdnHost: {uri.IdnHost}{Environment.NewLine}";

                Main.Text += $"IsAbsoluteUri: {uri.IsAbsoluteUri}{Environment.NewLine}";

                Main.Text += $"IsDefaultPort: {uri.IsDefaultPort}{Environment.NewLine}";

                Main.Text += $"IsFile: {uri.IsFile}{Environment.NewLine}";

                Main.Text += $"IsLoopback: {uri.IsLoopback}{Environment.NewLine}";

                Main.Text += $"IsUnc: {uri.IsUnc}{Environment.NewLine}";

                Main.Text += $"IsWellFormedOriginalString(): {uri.IsWellFormedOriginalString()}{Environment.NewLine}";

                Main.Text += $"LocalPath: {uri.LocalPath}{Environment.NewLine}";

                Main.Text += $"OriginalString: {uri.OriginalString}{Environment.NewLine}";

                Main.Text += $"PathAndQuery: {uri.PathAndQuery}{Environment.NewLine}";

                Main.Text += $"Port: {uri.Port}{Environment.NewLine}";

                Main.Text += $"Query: {uri.Query}{Environment.NewLine}";

                Main.Text += $"Scheme: {uri.Scheme}{Environment.NewLine}";

                Main.Text += $"Segments: {string.Join(", ", uri.Segments)}{Environment.NewLine}";

                Main.Text += $"UserEscaped: {uri.UserEscaped}{Environment.NewLine}";

                Main.Text += $"UserInfo: {uri.UserInfo}{Environment.NewLine}";
            }
            catch (Exception)
            {
                Main.Text = "";
            }

        }
    }
}
