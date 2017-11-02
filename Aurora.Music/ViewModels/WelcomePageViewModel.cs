using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Aurora.Music.ViewModels
{
    class WelcomePageViewModel : ViewModelBase
    {
        private double progress1;
        public double Progress1
        {
            get { return progress1; }
            set { SetProperty(ref progress1, value); }
        }

        private double progress2;
        public double Progress2
        {
            get { return progress2; }
            set { SetProperty(ref progress2, value); }
        }

        private double progress3;
        public double Progress3
        {
            get { return progress3; }
            set { SetProperty(ref progress3, value); }
        }

        private double progress4;
        public double Progress4
        {
            get { return progress4; }
            set { SetProperty(ref progress4, value); }
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
        private FileReader fileReader = new FileReader();

        public async Task StartSearch()
        {
            var set = Settings.Load();
            var list = new List<StorageFolder>();
            if (set.IncludeMusicLibrary)
            {
                list.Add(KnownFolders.MusicLibrary);
            }
            var p = await opr.GetAllAsync<FOLDER>();
            fileReader.ProgressUpdated += FileReader_ProgressUpdated;
            fileReader.Completed += FileReader_Completed;
            fileReader.NewSongsAdded += Opr_NewSongsAdded;
            foreach (var fo in p)
            {
                try
                {
                    list.Add(await StorageFolder.GetFolderFromPathAsync(fo.Path));
                }
                catch (Exception)
                {
                    continue;
                }
            }
            await fileReader.Read(list);
        }

        private async void Opr_NewSongsAdded(object sender, SongsAddedEventArgs e)
        {
            await fileReader.AddToAlbums(e.NewSongs);
        }

        private async void FileReader_Completed(object sender, EventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Finish = 1;
            });
        }

        private async void FileReader_ProgressUpdated(object sender, ProgressReport e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                switch (e.Stage)
                {
                    case 1:
                        Progress1 = Math.Round(e.Percent);
                        Hint = "Retrieving Files";
                        break;
                    case 2:
                        Progress2 = Math.Round(e.Percent);
                        Hint = $"Reading Tags: {Progress2}%";
                        break;
                    case 3:
                        Progress3 = Math.Round(e.Percent);
                        Hint = $"Updating Database: {Progress3}%";
                        break;
                    case 4:
                        Progress4 = Math.Round(e.Percent);
                        Hint = $"Sorting Albums: {Progress4}%";
                        break;
                    default:
                        break;
                }
            });
        }
    }
}
