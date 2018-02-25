// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Controls
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FileShare : Page
    {
        private Task completeTask;
        private AutoResetEvent finished;

        public FileShare()
        {
            this.InitializeComponent();
            finished = new AutoResetEvent(false);
            completeTask = Task.Run(() =>
            {
                finished.WaitOne();
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            finished.Set();
        }

        internal Task WaitForComplete()
        {
            return completeTask;
        }

        internal async Task FileRecieve(IReadOnlyList<IStorageItem> items)
        {
            var files = await FileReader.ReadFilesAsync(items);
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Music", CreationCollisionOption.OpenIfExists);

            int i = 1;
            foreach (var item in files)
            {
                await item.CopyAsync(folder, item.Name, NameCollisionOption.ReplaceExisting);
                Message.Text = SmartFormat.Smart.Format("{0} {0:file|files} copied", i);
                i++;
            }

            Message.Text += Environment.NewLine + $"these files are stored:{string.Join(", ", files.Select(a => a.Name))}";
        }
    }
}
