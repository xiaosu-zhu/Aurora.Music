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
        private FileReader fileReader = new FileReader();

        public async Task StartSearch()
        {
            List<StorageFolder> list = FileReader.InitFolderList();
            var p = await opr.GetAllAsync<FOLDER>();
            fileReader.ProgressUpdated += FileReader_ProgressUpdated;
            fileReader.Completed += FileReader_Completed;
            foreach (var fo in p)
            {
                try
                {
                    // TODO: folder attribute
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

            var s = Settings.Load();
            s.WelcomeFinished = true;
            s.Save();
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
