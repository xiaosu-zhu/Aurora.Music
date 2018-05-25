// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;

using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Controls
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LyricView : Page, INotifyPropertyChanged
    {
        private SongViewModel model;

        private LyricExtension extension;

        public LyricView()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
            ApplicationView.GetForCurrentView().Consolidated += LyricView_Consolidated;
            RequestedTheme = Settings.Current.Theme;
        }

        private void LyricView_Consolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
        {
            ApplicationView.GetForCurrentView().Consolidated -= LyricView_Consolidated;
            PlaybackEngine.PlaybackEngine.Current.PositionUpdated -= LyricView_PositionUpdated;
            PlaybackEngine.PlaybackEngine.Current.ItemsChanged -= LyricView_StatusChanged;
            MainPage.Current.LyricViewID = -1;
        }

        internal SongViewModel Model
        {
            get => model; set
            {
                SetProperty(ref model, value);
            }
        }

        internal LyricViewModel Lyric = new LyricViewModel();

        public event PropertyChangedEventHandler PropertyChanged;


        private void RaisePropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T backingField, T Value, [CallerMemberName] string propertyName = null)
        {
            var changed = !EqualityComparer<T>.Default.Equals(backingField, Value);
            if (changed)
            {
                backingField = Value;
                this.RaisePropertyChanged(propertyName);
            }
            return changed;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            PlaybackEngine.PlaybackEngine.Current.PositionUpdated += LyricView_PositionUpdated;
            PlaybackEngine.PlaybackEngine.Current.ItemsChanged += LyricView_StatusChanged;
            base.OnNavigatedTo(e);
            if (e.Parameter is SongViewModel m)
            {
                Model = m;

                extension = MainPageViewModel.Current.LyricExtension;
                if (extension != null)
                {
                    var result = await extension.GetLyricAsync(m.Song, MainPageViewModel.Current.OnlineMusicExtension?.ServiceName);
                    if (result != null)
                    {
                        var l = new Lyric(LrcParser.Parser.Parse((string)result, model.Song.Duration));
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            Lyric.New(l);
                        });
                    }
                    else
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            Lyric.Clear();
                        });
                    }
                }
                else
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        Lyric.Clear();
                    });
                }
            }
        }

        private async void LyricView_StatusChanged(object sender, PlaybackEngine.PlayingItemsChangedArgs e)
        {
            if (e.CurrentSong != null && !e.CurrentSong.IsIDEqual(Model.Song))
            {
                Model = new SongViewModel(e.CurrentSong);
                if (extension != null)
                {
                    var result = await extension.GetLyricAsync(e.CurrentSong, MainPageViewModel.Current.OnlineMusicExtension?.ServiceName);
                    if (result != null)
                    {
                        var l = new Lyric(LrcParser.Parser.Parse((string)result, model.Song.Duration));
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {

                            Lyric.New(l);
                        });
                    }
                    else
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            Lyric.Clear();
                        });
                    }
                }
                else
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        Lyric.Clear();
                    });
                }
            }
        }

        private async void LyricView_PositionUpdated(object sender, PlaybackEngine.PositionUpdatedArgs e)
        {
            if (Lyric != null && e.Current != default(TimeSpan))
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
              {
                  Lyric.Update(e.Current);
              });
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            PlaybackEngine.PlaybackEngine.Current.PositionUpdated -= LyricView_PositionUpdated;
            PlaybackEngine.PlaybackEngine.Current.ItemsChanged -= LyricView_StatusChanged;
        }
    }
}
