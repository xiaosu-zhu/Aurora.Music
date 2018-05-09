// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;

namespace Aurora.Music.ViewModels
{
    class WelcomePageViewModel : ViewModelBase
    {
        private double progress;
        public double Progress
        {
            get { return progress; }
            set { SetProperty(ref progress, value); }
        }

        private int finish = 0;
        public int Finish
        {
            get { return finish; }
            set { SetProperty(ref finish, value); }
        }

        private string hint = "Retrieving Files";
        public string Hint
        {
            get { return hint; }
            set { SetProperty(ref hint, value); }
        }

        private SQLOperator opr = SQLOperator.Current();

        public async Task StartSearch()
        {
            var list = FileReader.InitFolderList();
            var filtered = new List<string>();
            var p = await opr.GetAllAsync<FOLDER>();
            FileReader.ProgressUpdated += FileReader_ProgressUpdated;
            FileReader.Completed += FileReader_Completed;
            foreach (var fo in p)
            {
                try
                {
                    // TODO: folder attribute
                    var folder = await fo.GetFolderAsync();
                    if (list.Exists(a => a.Path == folder.Path))
                    {
                        continue;
                    }
                    if (fo.IsFiltered)
                    {
                        filtered.Add(folder.DisplayName);
                    }
                    else
                    {
                        list.Add(folder);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            try
            {
                list.Remove(list.Find(a => a.Path == ApplicationData.Current.LocalFolder.Path));
            }
            catch (Exception)
            {
            }
            await FileReader.ReadAsync(list, filtered);
        }

        private async void Opr_NewSongsAdded(object sender, SongsAddedEventArgs e)
        {
            await FileReader.SortAlbumsAsync();
        }

        private async void FileReader_Completed(object sender, EventArgs e)
        {
            Settings.Current.WelcomeFinished = true;
            Settings.Current.Save();
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Finish = 1;
            });
        }

        private async void FileReader_ProgressUpdated(object sender, ProgressReport e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Hint = e.Description;
                var t = (double)e.Total;
                if (e.Total == 0 || t == double.NaN)
                {
                    Progress = 0;
                }
                else
                {
                    Progress = ((100 * e.Current) / t);
                }
            });
        }
    }
}
