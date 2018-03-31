// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Music.ViewModels
{
    class DownloadPageViewModel : ViewModelBase
    {
        public ObservableCollection<DownloadItemViewModel> DownloadList { get; set; }

        private string totalDes;
        public string TotalDes
        {
            get { return totalDes; }
            set { SetProperty(ref totalDes, value); }
        }
        private double totalProgress;
        public double TotalProgress
        {
            get { return totalProgress; }
            set { SetProperty(ref totalProgress, value); }
        }

        public string TaskDesc(int count)
        {
            return SmartFormat.Smart.Format(Consts.Localizer.GetString("TaskDesc"), count);
        }

        public DownloadPageViewModel()
        {
            DownloadList = new ObservableCollection<DownloadItemViewModel>();

            var running = Downloader.Current.GetAll();
            foreach (var item in running)
            {
                DownloadList.Add(new DownloadItemViewModel()
                {
                    Title = item.Item2.Title,
                    Progress = item.Item1.Progress.TotalBytesToReceive == 0 ? 0 : item.Item1.Progress.BytesReceived * 100d / item.Item1.Progress.TotalBytesToReceive,
                    Status = item.Item1.Progress.Status,
                    Guid = item.Item1.Guid,
                    Description = item.Item2.Description
                });
            }
            double i = 0;
            foreach (var item in DownloadList)
            {
                i += item.Progress;
            }
            TotalProgress = DownloadList.Count == 0 ? 0 : (i / (DownloadList.Count));

            Downloader.Current.ProgressChanged += Current_ProgressChanged;
            Downloader.Current.ProgressCancelled += Current_ProgressCancelled;
            Downloader.Current.ItemCompleted += Current_ItemCompleted;
        }

        public void Unload()
        {
            Downloader.Current.ProgressChanged -= Current_ProgressChanged;
            Downloader.Current.ProgressCancelled -= Current_ProgressCancelled;
            Downloader.Current.ItemCompleted -= Current_ItemCompleted;
        }

        public string ProgressToString(double d)
        {
            return (d / 100d).ToString("P0");
        }

        private async void Current_ItemCompleted(object sender, (DownloadOperation, DownloadDesc) e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                try
                {
                    var p = DownloadList.First(a => a.Guid == e.Item1.Guid);
                    p.Status = e.Item1.Progress.Status;
                    p.Progress = e.Item1.Progress.TotalBytesToReceive == 0 ? 0 : e.Item1.Progress.BytesReceived * 100d / e.Item1.Progress.TotalBytesToReceive;
                }
                catch (Exception)
                {
                    DownloadList.Add(new DownloadItemViewModel()
                    {
                        Title = e.Item2.Title,
                        Progress = e.Item1.Progress.TotalBytesToReceive == 0 ? 0 : e.Item1.Progress.BytesReceived * 100d / e.Item1.Progress.TotalBytesToReceive,
                        Status = e.Item1.Progress.Status,
                        Guid = e.Item1.Guid,
                        Description = e.Item2.Description
                    });
                }
            });
        }

        private async void Current_ProgressCancelled(object sender, (DownloadOperation, DownloadDesc) e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
          {
              try
              {
                  var p = DownloadList.First(a => a.Guid == e.Item1.Guid);
                  p.Status = e.Item1.Progress.Status;
                  p.Progress = e.Item1.Progress.TotalBytesToReceive == 0 ? 0 : e.Item1.Progress.BytesReceived * 100d / e.Item1.Progress.TotalBytesToReceive;
              }
              catch (Exception)
              {
                  DownloadList.Add(new DownloadItemViewModel()
                  {
                      Title = e.Item2.Title,
                      Progress = e.Item1.Progress.TotalBytesToReceive == 0 ? 0 : e.Item1.Progress.BytesReceived * 100d / e.Item1.Progress.TotalBytesToReceive,
                      Status = e.Item1.Progress.Status,
                      Guid = e.Item1.Guid,
                      Description = e.Item2.Description
                  });
              }
          });
        }

        private async void Current_ProgressChanged(object sender, (DownloadOperation, DownloadDesc) e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                try
                {
                    var p = DownloadList.First(a => a.Guid == e.Item1.Guid);
                    p.Status = e.Item1.Progress.Status;
                    p.Progress = e.Item1.Progress.TotalBytesToReceive == 0 ? 0 : e.Item1.Progress.BytesReceived * 100d / e.Item1.Progress.TotalBytesToReceive;
                }
                catch (Exception)
                {
                    DownloadList.Add(new DownloadItemViewModel()
                    {
                        Title = e.Item2.Title,
                        Progress = e.Item1.Progress.TotalBytesToReceive == 0 ? 0 : e.Item1.Progress.BytesReceived * 100d / e.Item1.Progress.TotalBytesToReceive,
                        Status = e.Item1.Progress.Status,
                        Guid = e.Item1.Guid,
                        Description = e.Item2.Description
                    });
                }
                double i = 0;
                foreach (var item in DownloadList)
                {
                    i += item.Progress;
                }
                TotalProgress = DownloadList.Count == 0 ? 0 : (i / (DownloadList.Count));
            });
        }
    }

    class DownloadItemViewModel : ViewModelBase
    {
        private double progress;
        public double Progress
        {
            get { return progress; }
            set { SetProperty(ref progress, value); }
        }
        private string title;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public Guid Guid { get; internal set; }

        private string description;
        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value); }
        }

        private BackgroundTransferStatus status;
        public BackgroundTransferStatus Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }

        public string StatusToString(BackgroundTransferStatus s)
        {
            return s.GetDisplayName();
        }

        public string ProgressToString(double d)
        {
            return (d / 100).ToString("P0");
        }

        public double ProgressToRight(double d)
        {
            return 2.4 * d;
        }
        public double ProgressToLeft(double d)
        {
            return 2.4 * d - 240;
        }
    }
}
