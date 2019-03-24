// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.Core.Tools;
using Aurora.Music.Pages;
using Aurora.Music.ViewModels;
using Aurora.Shared;
using Aurora.Shared.Controls;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;

using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Aurora.Music
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, IChangeTheme
    {
        public static MainPage Current;

        internal object Lockable = new object();

        public MenuFlyout SongFlyout;
        private DataTransferManager dataTransferManager;

        public MainPage()
        {
            InitializeComponent();

            Current = this;
            SongFlyout = (Resources["SongFlyout"] as MenuFlyout);

            dataTransferManager = DataTransferManager.GetForCurrentView();
            Task.Run(async () =>
            {
                if (Settings.Current.AutoTheme)
                {
                    if (Settings.Current.SunTheme)
                    {
                        await RecalcSunTimeAsync();
                    }
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        AutoChangeTheme();
                    });
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        ChangeTheme(Settings.Current.Theme);
                    });
                }
            });
        }

        internal void SetSleepTimer(DateTime t, SleepAction a)
        {
            sleepTimer?.Cancel();
            sleepTime = t;
            sleepAction = a;
            var period = (t - DateTime.Now).Subtract(TimeSpan.FromSeconds(30));
            sleepTimer = ThreadPoolTimer.CreateTimer(async work =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                {
                    PopMessage($"In {(sleepTime - DateTime.Now).TotalSeconds.ToString("0")} seconds sleep timer will activate");
                    await Task.Delay(Convert.ToInt32((sleepTime - DateTime.Now).TotalMilliseconds));
                    switch (sleepAction)
                    {
                        case SleepAction.Pause:
                            MainPageViewModel.Current.PlayPause.Execute();
                            break;
                        case SleepAction.Stop:
                            MainPageViewModel.Current.Stop.Execute();
                            break;
                        case SleepAction.Shutdown:
                            Application.Current.Exit();
                            break;
                        default:
                            break;
                    }
                });
            }, period.TotalSeconds < 0 ? TimeSpan.FromSeconds(1) : period, destroy =>
            {
            });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;

            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;


            if (e.Parameter is ValueTuple<Type, Type, int, string> m)
            {
                while (HamPane.Items.Count <= 0)
                {
                    await Task.Delay(500);
                }

                MainFrame.Navigate(m.Item1, (m.Item2, m.Item3, m.Item4));
            }
            else
            {
                while (HamPane.Items.Count <= 0)
                {
                    await Task.Delay(500);
                }
                HamPane.SelectedIndex = 0;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            //SystemNavigationManager.GetForCurrentView().BackRequested -= MaiPage_BackRequested;
            //dataTransferManager.DataRequested -= DataTransferManager_DataRequested;
        }

        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (e.Handled || ((Window.Current.Content is Frame f) && f.Content is CompactOverlayPanel)) return;


            if (OverlayFrame.Visibility == Visibility.Visible && OverlayFrame.Content is IRequestGoBack g)
            {
                e.Handled = true;
                g.RequestGoBack();
                return;
            }
            if (MainFrame.Visibility == Visibility.Visible && MainFrame.Content is IRequestGoBack p)
            {
                e.Handled = true;
                p.RequestGoBack();
                return;
            }

            e.Handled = GoBack();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.SetText($"{shareTitle} - {shareDesc}");
            args.Request.Data.Properties.Title = shareTitle;
            args.Request.Data.Properties.Description = shareDesc;
        }

        public async void ProgressUpdate(string title, string content)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                ProgressUpdateTitle.Text = title;
                ProgressUpdateContent.Text = content;
            });
        }

        private bool _show;

        public async void ProgressUpdate(bool show = true)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (_show != show)
                {
                    _show = show;
                    if (show)
                    {
                        ProgressUpdateNotify.Show();
                    }
                    else
                    {
                        ProgressUpdateNotify.Dismiss();
                    }
                }
            });

        }

        /// <summary>
        /// 0 to 100
        /// </summary>
        /// <param name="progress">0 to 100</param>
        public async void ProgressUpdate(double progress)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                ProgressUpdateProgress.Value = progress;
            });
        }

        internal bool GoBack()
        {
            if (OverlayFrame.Visibility == Visibility.Visible)
            {
                return false;
            }
            else
            {
                if (MainFrame.CanGoBack)
                {
                    MainFrame.GoBack();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal async void ThrowException(Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                InAppNotify.Content = "  Error occured: " + e.Message + "\r\n- " + e.Exception.GetType().ToString();
                InAppNotify.Show();
            });
            dismissTimer?.Cancel();
            dismissTimer = ThreadPoolTimer.CreateTimer(async (x) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    InAppNotify.Dismiss();
                });
            }, TimeSpan.FromMilliseconds(3000));
        }

        public int LyricViewID = -1;
        private StackPanel autoSuggestPopupPanel;
        private ThreadPoolTimer dismissTimer;
        private string shareTitle;
        private string shareDesc;
        private DateTime sleepTime;
        private SleepAction sleepAction;
        private ThreadPoolTimer sleepTimer;
        private ThreadPoolTimer dropTimer;
        private Rect sizeBefore;

        public bool IsCurrentDouban => MainFrame.Content is DoubanPage;

        public bool CanShowPanel => !(OverlayFrame.Visibility == Visibility.Visible || MainFrame.Content is DoubanPage);

        public bool IsInAppDrag { get; set; }

        string PositionToString(TimeSpan t1, TimeSpan total)
        {
            if (total == null || total == default(TimeSpan))
            {
                return "0:00/0:00";
            }
            return $"{$"{(int)(Math.Floor(t1.TotalMinutes))}{CultureInfoHelper.CurrentCulture.DateTimeFormat.TimeSeparator}{t1.Seconds.ToString("00")}"}/{$"{(int)(Math.Floor(total.TotalMinutes))}{CultureInfoHelper.CurrentCulture.DateTimeFormat.TimeSeparator}{total.Seconds.ToString("00")}"}";
        }

        string PositionNarrowToString(TimeSpan t1)
        {
            return $"{(int)(Math.Floor(t1.TotalMinutes))}{CultureInfoHelper.CurrentCulture.DateTimeFormat.TimeSeparator}{t1.Seconds.ToString("00")}";
        }

        public void Navigate(Type type)
        {
            if (OverlayFrame.Visibility == Visibility.Visible)
                return;
            MainFrame.Navigate(type);
            if (Settings.Current.AutoTheme)
            {
                AutoChangeTheme();
            }
        }

        public void Navigate(Type type, object parameter)
        {
            if (OverlayFrame.Visibility == Visibility.Visible)
                return;
            MainFrame.Navigate(type, parameter);
            if (Settings.Current.AutoTheme)
            {
                AutoChangeTheme();
            }
        }

        public Orientation PaneToOrientation(bool a)
        {
            return a ? Orientation.Horizontal : Orientation.Vertical;
        }

        public void ChangeTheme()
        {
            if (MainFrame.Content is IChangeTheme iT)
            {
                iT.ChangeTheme();
            }
            var ui = new UISettings();
            Context.IsDarkAccent = Palette.IsDarkColor(ui.GetColorValue(UIColorType.Accent));
        }

        internal async Task RecalcSunTimeAsync()
        {
            var loc = await Helper.GetLocationAsync();
            if (loc.lat is double lat && loc.lng is double lng)
            {
                Settings.Current.Longitude = lng;
                Settings.Current.Latitude = lat;
            }
            var (tsunrise, tsunset) = Sunriset.SunriseSunset(DateTime.Now, (Settings.Current.Longitude, Settings.Current.Latitude));
            Settings.Current.RiseTime = tsunrise.TotalSeconds;
            Settings.Current.FallTime = tsunset.TotalSeconds;
            Settings.Current.Save();
        }

        internal void AutoChangeTheme()
        {
            var elapsed = DateTime.Now - DateTime.Today;
            if (Settings.Current.FallTime > Settings.Current.RiseTime)
            {
                // normal dark and light
                if (elapsed.TotalSeconds > Settings.Current.RiseTime && elapsed.TotalSeconds < Settings.Current.FallTime)
                {
                    ChangeTheme(ElementTheme.Light);
                }
                else
                {
                    ChangeTheme(ElementTheme.Dark);
                }
            }
            else
            {
                // dark light inverted
                if (elapsed.TotalSeconds > Settings.Current.FallTime && elapsed.TotalSeconds < Settings.Current.RiseTime)
                {
                    ChangeTheme(ElementTheme.Dark);
                }
                else
                {
                    ChangeTheme(ElementTheme.Light);
                }
            }
        }

        public void ChangeTheme(ElementTheme theme)
        {
            if (MainFrame.Content is IChangeTheme iT)
            {
                iT.ChangeTheme(theme);
            }
            RequestedTheme = theme;
            var ui = new UISettings();
            Context.IsDarkAccent = Palette.IsDarkColor(ui.GetColorValue(UIColorType.Accent));

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            // titleBar.ButtonHoverBackgroundColor = Colors.Red;
            switch (theme)
            {
                case ElementTheme.Default:
                    titleBar.ButtonInactiveForegroundColor = (Color)Resources["SystemBaseLowColor"];
                    titleBar.ButtonForegroundColor = (Color)Resources["SystemBaseHighColor"];
                    titleBar.ButtonHoverForegroundColor = (Color)Resources["SystemAltHighColor"];
                    titleBar.ButtonHoverBackgroundColor = (Color)Resources["SystemBaseLowColor"];
                    break;
                case ElementTheme.Light:
                    titleBar.ButtonInactiveForegroundColor = Color.FromArgb(0x33, 0, 0, 0);
                    titleBar.ButtonForegroundColor = Color.FromArgb(0xff, 0, 0, 0);
                    titleBar.ButtonHoverForegroundColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x33, 0, 0, 0);
                    break;
                case ElementTheme.Dark:
                    titleBar.ButtonInactiveForegroundColor = Color.FromArgb(0x33, 0xff, 0xff, 0xff);
                    titleBar.ButtonForegroundColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
                    titleBar.ButtonHoverForegroundColor = Color.FromArgb(0xff, 0, 0, 0);
                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x33, 0xff, 0xff, 0xff);
                    break;
                default:
                    titleBar.ButtonInactiveForegroundColor = (Color)Resources["SystemBaseLowColor"];
                    titleBar.ButtonForegroundColor = (Color)Resources["SystemBaseHighColor"];
                    titleBar.ButtonHoverForegroundColor = (Color)Resources["SystemAltHighColor"];
                    titleBar.ButtonHoverBackgroundColor = (Color)Resources["SystemBaseLowColor"];
                    break;
            }
        }

        private void MainPage_Completed(ConnectedAnimation sender, object args)
        {
            NowPanel.Visibility = Visibility.Collapsed;
            sender.Completed -= MainPage_Completed;
        }

        public void GoBackFromNowPlaying()
        {
            if (OverlayFrame.Visibility == Visibility.Visible)
            {
                NowPanel.Visibility = Visibility.Visible;
                MainFrame.Visibility = Visibility.Visible;

                if (MainFrame.Content is HomePage) Context.NeedShowBack = false;

                (OverlayFrame.Content as NowPlayingPage).Unload();
                OverlayFrame.Content = null;
                var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.NowPlayingPageInAnimation);
                if (ani != null)
                {
                    ani.TryStart(Artwork, new UIElement[] { Root });
                }
                ani = ConnectedAnimationService.GetForCurrentView().GetAnimation($"{Consts.NowPlayingPageInAnimation}_1");
                if (ani != null)
                {
                    ani.TryStart(Title);
                }
                ani = ConnectedAnimationService.GetForCurrentView().GetAnimation($"{Consts.NowPlayingPageInAnimation}_2");
                if (ani != null)
                {
                    ani.TryStart(Album);

                    ani.Completed += Ani_Completed;
                }
                else
                {
                    OverlayFrame.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Ani_Completed(ConnectedAnimation sender, object args)
        {
            sender.Completed -= Ani_Completed;
            OverlayFrame.Visibility = Visibility.Collapsed;
            Context.RestoreLastTitle();
        }


        internal async void PopMessage(string msg)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                InAppNotify.Show(msg, 3000);
            });
        }
        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(TitleBar);
        }

        internal async Task ShowLyricWindow()
        {
            if (Settings.Current.Singleton && LyricViewID != -1)
            {
                // TODO: this won't work
                await ApplicationViewSwitcher.SwitchAsync(LyricViewID, ApplicationView.GetForCurrentView().Id, ApplicationViewSwitchingOptions.Default);
                return;
            }
            await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var frame = new Frame();
                LyricViewID = ApplicationView.GetForCurrentView().Id;
                frame.Navigate(typeof(LyricView), Context.NowPlayingList[Context.CurrentIndex]);
                Window.Current.Content = frame;
                Window.Current.Activate();
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                Window.Current.SetTitleBar(frame);
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                // titleBar.ButtonHoverBackgroundColor = Colors.Red;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x33, 0x00, 0x00, 0x00);
                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonInactiveForegroundColor = Colors.Gray;
            });
            var compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            compactOptions.CustomSize = new Size(1000, 100);
            compactOptions.ViewSizePreference = ViewSizePreference.Custom;
            bool viewShown = await ApplicationViewSwitcher.TryShowAsViewModeAsync(LyricViewID, ApplicationViewMode.CompactOverlay, compactOptions);
        }

        internal void HideAutoSuggestPopup()
        {
            autoSuggestPopupPanel.Children[1].Visibility = Visibility.Collapsed;
            ((autoSuggestPopupPanel.Children[1] as Panel).Children[0] as ProgressRing).IsActive = false;
        }

        internal async Task GotoComapctOverlay()
        {
            sizeBefore = Window.Current.Bounds;
            var prefer = ViewModePreferences.CreateDefault(Settings.Current.DontOverlay ? ApplicationViewMode.Default : ApplicationViewMode.CompactOverlay);
            prefer.ViewSizePreference = ViewSizePreference.Custom;
            prefer.CustomSize = new Size(Settings.Current.CompactWidth, Settings.Current.CompactHeight);
            if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(Settings.Current.DontOverlay ? ApplicationViewMode.Default : ApplicationViewMode.CompactOverlay, prefer))
            {
                (Window.Current.Content as Frame).Navigate(typeof(CompactOverlayPanel), Context.NowPlayingList[Context.CurrentIndex]);
                ApplicationView.GetForCurrentView().TryResizeView(new Size(Settings.Current.CompactWidth, Settings.Current.CompactHeight));
            }
        }

        internal void ShowAutoSuggestPopup()
        {
            autoSuggestPopupPanel.Children[1].Visibility = Visibility.Visible;
            ((autoSuggestPopupPanel.Children[1] as Panel).Children[0] as ProgressRing).IsActive = true;
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {

        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
        }

        internal void ShowPodcast(string ID)
        {
            if (MainFrame.Content is LibraryPage l)
            {
                l.ShowPodcast(ID);
            }
            else
            {
                MainFrame.Navigate(typeof(LibraryPage), ID);
            }
        }

        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (sender.Text.Equals("Aurora Music", StringComparison.InvariantCultureIgnoreCase))
            {
                Magic.Text = await FileIOHelper.ReadStringFromAssetsAsync("Others/art.txt");
                MagicBorder.Visibility = Visibility.Visible;
            }
            if (args.ChosenSuggestion is GenericMusicItemViewModel g)
            {
                if (g.Title.IsNullorEmpty())
                {
                    return;
                }
                if (g.InnerType == MediaType.Placeholder)
                {
                    SearchBox.Text = g.Title;
                    return;
                }
                else if (g.InnerType == MediaType.Album)
                {
                    var view = new AlbumViewDialog(await g.FindAssociatedAlbumAsync());
                    var result = await view.ShowAsync();
                }
                else if (g.InnerType == MediaType.Podcast)
                {
                    var view = new PodcastDialog(g);
                    var result = await view.ShowAsync();
                }
                else
                {
                    var t = Task.Run(async () =>
                    {
                        await SQLOperator.Current().SaveSearchHistoryAsync(g.Title);
                    });
                    var dialog = new SearchResultDialog(g);
                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Secondary)
                    {
                        ShowModalUI(true, Consts.Localizer.GetString("WaitingResultText"));
                        var view = new AlbumViewDialog(await g.FindAssociatedAlbumAsync());
                        ShowModalUI(false);
                        result = await view.ShowAsync();
                    }
                }
            }
            else
            {
                if (Context.SearchItems.IsNullorEmpty())
                {
                    var dialog = new SearchResultDialog();
                    var result = await dialog.ShowAsync();
                }
                else
                {
                    if (Context.SearchItems[0].InnerType == MediaType.Placeholder)
                    {
                        SearchBox.Text = Context.SearchItems[0].Title;
                        return;
                    }
                    var t = Task.Run(async () =>
                    {
                        await SQLOperator.Current().SaveSearchHistoryAsync(Context.SearchItems[0].Title);
                    });
                    if (Context.SearchItems[0].InnerType == MediaType.Album)
                    {
                        var view = new AlbumViewDialog(await Context.SearchItems[0].FindAssociatedAlbumAsync());
                        var result = await view.ShowAsync();
                    }
                    else
                    {
                        var dialog = new SearchResultDialog(Context.SearchItems[0]);
                        var result = await dialog.ShowAsync();
                        if (result == ContentDialogResult.Secondary)
                        {
                            ShowModalUI(true, Consts.Localizer.GetString("WaitingResultText"));
                            var view = new AlbumViewDialog(await Context.SearchItems[0].FindAssociatedAlbumAsync());
                            ShowModalUI(false);
                            result = await view.ShowAsync();
                        }
                    }
                }
            }
            sender.Text = string.Empty;
        }

        private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if ((args.SelectedItem as GenericMusicItemViewModel).Title.IsNullorEmpty())
            {
            }
        }

        private async void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen)
            {
                if (!Context.SearchItems.IsNullorEmpty())
                {
                    if (Context.SearchItems[0].InnerType == MediaType.Placeholder)
                    {

                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            if (sender.Text.IsNullorWhiteSpace())
            {
                Context.SearchItems.Clear();
                return;
            }
            var text = sender.Text;

            text = text.Replace('\'', ' ');
            await Context.SearchAsync(text, args);
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            Context.SkiptoItem((sender as Button).DataContext as SongViewModel);
        }

        private async void Root_DragOver(object sender, DragEventArgs e)
        {
            if (IsInAppDrag)
            {
                return;
            }
            var d = e.GetDeferral();
            var p = await e.DataView.GetStorageItemsAsync();
            if (p.Count > 0 && IsSongsFile(p))
            {
                e.Handled = true;
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.Caption = Consts.Localizer.GetString("DroptoPlay");
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsContentVisible = true;
                e.AcceptedOperation = DataPackageOperation.None | DataPackageOperation.Copy | DataPackageOperation.Link | DataPackageOperation.Move;
            }
            else
            {
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.Caption = Consts.Localizer.GetString("NotSupport");
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsContentVisible = false;
            }
            d.Complete();
        }

        private bool IsSongsFile(IReadOnlyList<IStorageItem> p)
        {
            foreach (var item in p)
            {
                if (item is IStorageFile file)
                {
                    foreach (var types in Consts.FileTypes)
                    {
                        if (types == file.FileType)
                        {
                            return true;
                        }
                    }
                }
                else if (item is IStorageFolder f)
                {
                    return true;
                }
            }
            return false;
        }

        private async void Root_Drop(object sender, DragEventArgs e)
        {
            var d = e.GetDeferral();
            var p = await e.DataView.GetStorageItemsAsync();
            if (p.Count > 0 && IsSongsFile(p))
            {
                e.Handled = true;
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.Caption = "Drop to Play";
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsContentVisible = true;
                e.AcceptedOperation = DataPackageOperation.None | DataPackageOperation.Copy | DataPackageOperation.Link | DataPackageOperation.Move;
                await FileActivation(p);
            }
            else
            {
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.Caption = "Not Support";
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsContentVisible = false;
            }

            var point = e.GetPosition(Root);
            d.Complete();

            DropHint.Margin = new Thickness(point.X - DropHint.Width / 2, point.Y - DropHint.Height / 2, Root.ActualWidth - point.X - DropHint.Width / 2, Root.ActualHeight - point.Y - DropHint.Height / 2);
            DropHint.Visibility = Visibility.Visible;
            dropTimer?.Cancel();
            dropTimer = null;
        }

        private async Task FileActivation(IReadOnlyList<IStorageItem> p)
        {
            ShowModalUI(true, Consts.Localizer.GetString("LoadingFiles"));

            var list = new List<StorageFile>();
            if (p.Count > 0)
            {
                list.AddRange(await FileReader.ReadFilesAsync(p));
            }
            else
            {
                ShowModalUI(false);
                return;
            }
            if (list.Count < 1)
            {
                ShowModalUI(false);
                return;
            }

            await Context.InstantPlayAsync(list);


            if (list.Count > 0)
            {
                ShowModalUI(false);
                if (Settings.Current.RememberFileActivatedAction)
                {
                    if (Settings.Current.CopyFileWhenActivated)
                    {
                        var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Music", CreationCollisionOption.OpenIfExists);
                        foreach (var item in list)
                        {
                            try
                            {
                                await item.CopyAsync(folder, item.Name, NameCollisionOption.ReplaceExisting);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
                else
                {
                    ShowDropSongsUI(list);
                }
            }
        }

        private async void ShowDropSongsUI(List<StorageFile> files)
        {
            var dialog = new DropSongsDialog(files);
            var result = await dialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.None:
                    break;
                case ContentDialogResult.Primary:
                    var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Music", CreationCollisionOption.OpenIfExists);
                    foreach (var item in files)
                    {
                        try
                        {
                            await item.CopyAsync(folder, item.Name, NameCollisionOption.ReplaceExisting);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    break;
                case ContentDialogResult.Secondary:
                    break;
                default:
                    break;
            }
        }

        public void FileActivated(IReadOnlyList<IStorageItem> p)
        {
            var t = Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                await FileActivation(p);
            });
        }

        public async void ShowModalUI(bool show, string title = "")
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (show)
                {
                    ModalIn.Begin();
                }
                else
                {
                    ModalOut.Begin();
                }
                ModalText.Text = title;
            });
        }

        private void SearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            var box = sender as AutoSuggestBox;
            var up = box.GetFirst<Popup>();
            autoSuggestPopupPanel = up.Child as StackPanel;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.LayoutMetricsChanged -= CoreTitleBar_LayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged -= CoreTitleBar_IsVisibleChanged;
            GC.Collect();
        }


        private async void SearchBox_GettingFocus(UIElement sender, Windows.UI.Xaml.Input.GettingFocusEventArgs args)
        {
            if (args.OldFocusedElement is Button)
            {
                args.TryCancel();
                return;
            }

            if (args.FocusState != FocusState.Pointer)
            {
                return;
            }
            args.Handled = true;
            if (SearchBox.Text.IsNullorEmpty())
            {
                Context.SearchItems.Clear();

                // add clipboard text
                var dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    string text = await dataPackageView.GetTextAsync();
                    if (!string.IsNullOrWhiteSpace(text))
                        Context.SearchItems.Add(new GenericMusicItemViewModel()
                        {
                            Title = text,
                            InnerType = MediaType.Placeholder,
                            Description = "\uE16D",
                            IsSearch = true
                        });
                }

                // add search history
                var searches = await SQLOperator.Current().GetSearchHistoryAsync();
                foreach (var item in searches)
                {
                    Context.SearchItems.Add(new GenericMusicItemViewModel()
                    {
                        Title = item.Query,
                        InnerType = MediaType.Placeholder,
                        Description = "\uE81C",
                        IsSearch = true
                    });
                }
            }
            if (!SearchBox.Items.IsNullorEmpty())
            {
                SearchBox.IsSuggestionListOpen = true;
            }
            else
            {
                SearchBox.IsSuggestionListOpen = false;
            }
        }

        private void SearchBox_LosingFocus(UIElement sender, Windows.UI.Xaml.Input.LosingFocusEventArgs args)
        {
            if ((args.NewFocusedElement is SelectorItem t && t.Content is GenericMusicItemViewModel g && g.IsSearch) || (args.NewFocusedElement is FrameworkElement f && f.DataContext is GenericMusicItemViewModel n && n.IsSearch))
            {
                if (Context.SearchItems[0].InnerType == MediaType.Placeholder)
                {
                    args.Cancel = true;
                    args.Handled = true;
                    return;
                }
            }
        }

        private void NowPanel_Click(object sender, RoutedEventArgs e)
        {
            if (Context.NowPlayingList.Count > 0 && Context.CurrentIndex >= 0)
            {
                OverlayFrame.Visibility = Visibility.Visible;
                MainFrame.Visibility = Visibility.Collapsed;
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.NowPlayingPageInAnimation, Artwork);
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate($"{Consts.NowPlayingPageInAnimation}_1", Title);
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate($"{Consts.NowPlayingPageInAnimation}_2", Album).Completed += MainPage_Completed; ;
                OverlayFrame.Navigate(typeof(NowPlayingPage), Context.NowPlayingList[Context.CurrentIndex]);
            }
        }

        private async void MenuFlyoutPlay_Click(object sender, RoutedEventArgs e)
        {
            if (SongFlyout.Target is SelectorItem s)
            {
                switch (s.Content)
                {
                    case GenericMusicItemViewModel g:
                        await Context.InstantPlayAsync(await g.GetSongsAsync());
                        break;
                    case SongViewModel song:
                        await Context.InstantPlayAsync(new List<Song>() { song.Song });
                        break;
                    case AlbumViewModel album:
                        await Context.InstantPlayAsync(await album.GetSongsAsync());
                        break;
                    case ArtistViewModel artist:
                        await Context.InstantPlayAsync(await artist.GetSongsAsync());
                        break;

                    default:
                        break;
                }
            }
        }

        internal async void RestoreFromCompactOverlay()
        {
            var prefer = ViewModePreferences.CreateDefault(ApplicationViewMode.Default);
            prefer.ViewSizePreference = ViewSizePreference.Custom;
            prefer.CustomSize = new Size(sizeBefore.Width, sizeBefore.Height);
            if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default, prefer))
            {
                (Window.Current.Content as Frame).GoBack();
                try
                {
                    Context.RestoreFromCompactOverlay();
                    ApplicationView.GetForCurrentView().TryResizeView(new Size(sizeBefore.Width, sizeBefore.Height));
                    Window.Current.SetTitleBar(null);
                    Window.Current.SetTitleBar(TitleBar);
                }
                catch (Exception)
                {

                }
            }
        }

        private async void MenuFlyoutPlayNext_Click(object sender, RoutedEventArgs e)
        {
            if (SongFlyout.Target is SelectorItem s)
            {
                switch (s.Content)
                {
                    case GenericMusicItemViewModel g:
                        await Context.PlayNextAsync(await g.GetSongsAsync());
                        break;
                    case SongViewModel song:
                        await Context.PlayNextAsync(new List<Song>() { song.Song });
                        break;
                    case AlbumViewModel album:
                        await Context.PlayNextAsync(await album.GetSongsAsync());
                        break;
                    case ArtistViewModel artist:
                        await Context.PlayNextAsync(await artist.GetSongsAsync());
                        break;
                    default:
                        break;
                }
            }
        }
        private async void MenuFlyoutAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (SongFlyout.Target is SelectorItem s)
            {
                AlbumViewModel viewModel = null;
                switch (s.Content)
                {
                    case GenericMusicItemViewModel g:
                        viewModel = await g.FindAssociatedAlbumAsync();
                        break;
                    case SongViewModel song:
                        viewModel = await song.GetAlbumAsync();
                        break;
                    case AlbumViewModel album:
                        viewModel = album;
                        break;
                    case ArtistViewModel artist:
                        break;

                    default:
                        break;
                }
                var dialog = new AlbumViewDialog(viewModel);
                await dialog.ShowAsync();
            }
        }

        public async void MenuFlyoutArtist_Click(object sender, RoutedEventArgs e)
        {
            var artist = (sender as MenuFlyoutItem).Text;
            var dialog = new ArtistViewDialog(new ArtistViewModel()
            {
                Name = artist,
            });
            await dialog.ShowAsync();
        }

        private void MenuFlyoutShare_Click(object sender, RoutedEventArgs e)
        {
            if (SongFlyout.Target is SelectorItem s)
            {
                switch (s.Content)
                {
                    case GenericMusicItemViewModel g:
                        shareTitle = $"I'm sharing {g.Title} to you";
                        shareDesc = $"{g.ToString()}";
                        break;
                    case SongViewModel song:
                        shareTitle = $"I'm sharing {song.Title} to you";
                        shareDesc = $"{song.ToString()}";
                        break;
                    case AlbumViewModel album:
                        shareTitle = $"I'm sharing {album.Name} to you";
                        shareDesc = string.Format(Consts.Localizer.GetString("TileDesc"), album.Name, album.GetFormattedArtists());
                        break;
                    case ArtistViewModel artist:
                        shareTitle = $"I'm sharing {artist.Name} to you";
                        shareDesc = $"";
                        break;
                    default:
                        break;
                }
            }
            DataTransferManager.ShowShareUI();
        }

        public void Share(SongViewModel g)
        {
            shareTitle = $"I'm sharing {g.Title} to you";
            shareDesc = $"{g.ToString()}";
            DataTransferManager.ShowShareUI();
        }

        internal void Share(List<SongViewModel> s)
        {
            shareTitle = $"I'm sharing {SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), s.Count)} to you";
            shareDesc = string.Join(Environment.NewLine, s.Select(m => m.ToString()));
            DataTransferManager.ShowShareUI();
        }

        private async void MenuFlyoutModify_Click(object sender, RoutedEventArgs e)
        {
            if (SongFlyout.Target is SelectorItem s)
            {
                switch (s.Content)
                {
                    case GenericMusicItemViewModel g:
                        switch (g.InnerType)
                        {
                            case MediaType.Song:
                                if (g.IsOnline)
                                {
                                    throw new InvalidOperationException("Can't open an online file");
                                }
                                await new TagDialog(new SongViewModel((await g.GetSongsAsync())[0])).ShowAsync();
                                break;
                            case MediaType.Album:
                                PopMessage("Not support for this kind");
                                break;
                            case MediaType.PlayList:
                                PopMessage("Not support for this kind");
                                break;
                            case MediaType.Artist:
                                PopMessage("Not support for this kind");
                                break;
                            default:
                                break;
                        }
                        break;
                    case SongViewModel song:
                        await new TagDialog(song).ShowAsync();
                        break;
                    case AlbumViewModel album:
                        PopMessage("Not support for this kind");
                        break;
                    case ArtistViewModel artist:
                        PopMessage("Not support for this kind");
                        break;

                    default:
                        break;
                }
            }
        }
        private async void MenuFlyoutRevealExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (SongFlyout.Target is SelectorItem s)
            {
                string path = null;
                switch (s.Content)
                {
                    case GenericMusicItemViewModel g:
                        switch (g.InnerType)
                        {
                            case MediaType.Song:
                                if (g.IsOnline)
                                {
                                    throw new InvalidOperationException("Can't open an online file");
                                }
                                path = (await g.GetSongsAsync())[0].FilePath;
                                break;
                            case MediaType.Album:
                                break;
                            case MediaType.PlayList:
                                break;
                            case MediaType.Artist:
                                break;
                            default:
                                break;
                        }
                        break;
                    case SongViewModel song:
                        path = song.FilePath;
                        break;
                    case AlbumViewModel album:
                        break;
                    case ArtistViewModel artist:
                        break;

                    default:
                        break;
                }
                if (!path.IsNullorEmpty())
                {
                    var file = await StorageFile.GetFileFromPathAsync(path);
                    var option = new FolderLauncherOptions();
                    option.ItemsToSelect.Add(file);
                    await Launcher.LaunchFolderAsync(await file.GetParentAsync(), option);
                }
            }
        }
        private async void MenuFlyoutDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SongFlyout.Target is SelectorItem s)
            {
                var paths = new List<string>();
                switch (s.Content)
                {
                    case GenericMusicItemViewModel g:
                        switch (g.InnerType)
                        {
                            case MediaType.Song:
                                if (g.IsOnline)
                                {
                                    throw new InvalidOperationException("Can't open an online file");
                                }
                                paths.Add((await g.GetSongsAsync())[0].FilePath);
                                break;
                            case MediaType.Album:
                                paths.AddRange((await g.GetSongsAsync()).Select(x => x.FilePath));
                                break;
                            case MediaType.PlayList:
                                break;
                            case MediaType.Artist:
                                break;
                            default:
                                break;
                        }
                        break;
                    case SongViewModel song:
                        paths.Add(song.FilePath);
                        break;
                    case AlbumViewModel album:
                        paths.AddRange((await album.GetSongsAsync()).Select(x => x.FilePath));
                        break;
                    case ArtistViewModel artist:
                        break;

                    default:
                        break;
                }
                foreach (var path in paths)
                {
                    if (path.IsNullorEmpty()) continue;
                    try
                    {
                        var file = await StorageFile.GetFileFromPathAsync(path);
                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            PopMessage("Deleted");
        }
        private async void MenuFlyoutTrash_Click(object sender, RoutedEventArgs e)
        {

            if (SongFlyout.Target is SelectorItem s)
            {
                var paths = new List<string>();
                switch (s.Content)
                {
                    case GenericMusicItemViewModel g:
                        switch (g.InnerType)
                        {
                            case MediaType.Song:
                                if (g.IsOnline)
                                {
                                    throw new InvalidOperationException("Can't open an online file");
                                }
                                paths.Add((await g.GetSongsAsync())[0].FilePath);
                                break;
                            case MediaType.Album:
                                paths.AddRange((await g.GetSongsAsync()).Select(x => x.FilePath));
                                break;
                            case MediaType.PlayList:
                                break;
                            case MediaType.Artist:
                                break;
                            default:
                                break;
                        }
                        break;
                    case SongViewModel song:
                        paths.Add(song.FilePath);
                        break;
                    case AlbumViewModel album:
                        paths.AddRange((await album.GetSongsAsync()).Select(x => x.FilePath));
                        break;
                    case ArtistViewModel artist:
                        break;

                    default:
                        break;
                }
                foreach (var path in paths)
                {
                    if (path.IsNullorEmpty()) continue;
                    try
                    {
                        var file = await StorageFile.GetFileFromPathAsync(path);
                        await file.DeleteAsync(StorageDeleteOption.Default);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            PopMessage("Deleted");
        }

        private async void MenuFlyoutCollection_Click(object sender, RoutedEventArgs e)
        {
            if (SongFlyout.Target is SelectorItem s)
            {
                AddPlayList dialog;
                switch (s.Content)
                {
                    case GenericMusicItemViewModel g:
                        dialog = new AddPlayList((await g.GetSongsAsync()).Select(x => x.ID));
                        break;
                    case SongViewModel song:
                        dialog = new AddPlayList(song.ID);
                        break;
                    case AlbumViewModel album:
                        dialog = new AddPlayList((await album.GetSongsAsync()).Select(x => x.ID));
                        break;
                    case ArtistViewModel artist:
                        dialog = new AddPlayList((await artist.GetSongsAsync()).Select(x => x.ID));
                        break;
                    default:
                        throw new OperationCanceledException();
                }
                await dialog.ShowAsync();
            }
        }

        private async void Flyout_Opened(object sender, object e)
        {
            await NowPlayingFlyout.ScrollToIndex(NowPlayingFlyout.SelectedIndex, ScrollPosition.Center);
        }

        private void NowPanel_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ProgressShow.Begin();
        }

        private void NowPanel_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ProgressHide.Begin();
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {

        }

        private void KeyboardAccelerator_Invoked(Windows.UI.Xaml.Input.KeyboardAccelerator sender, Windows.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            var item = (args.Element as Panel).DataContext as HamPanelItem;
            if (OverlayFrame.Visibility == Visibility.Visible)
            {
                GoBackFromNowPlaying();
            }

            if (MainFrame.Content is SettingsPage || MainFrame.Content is AboutPage)
            {
                MainFrame.Navigate(item.TargetType);
                return;
            }

            var index = HamPane.SelectedIndex;

            if (index >= 0 && item == Context.HamList[index])
            {
                return;
            }
            MainFrame.Navigate(item.TargetType);
        }

        private void Panel_AccessKeyInvoked(UIElement sender, Windows.UI.Xaml.Input.AccessKeyInvokedEventArgs args)
        {
            var item = (sender as Panel).DataContext as HamPanelItem;
            if (OverlayFrame.Visibility == Visibility.Visible)
            {
                GoBackFromNowPlaying();
            }

            if (MainFrame.Content is SettingsPage || MainFrame.Content is AboutPage)
            {
                MainFrame.Navigate(item.TargetType);
                return;
            }

            var index = HamPane.SelectedIndex;
            if (index >= 0 && item == Context.HamList[index])
            {
                return;
            }
            MainFrame.Navigate(item.TargetType);
        }

        private async void Artwork_ImageOpened(object sender, RoutedEventArgs e)
        {
            if (OverlayFrame.Visibility == Visibility.Collapsed)
            {
                dropTimer?.Cancel();
                await Task.Delay(200);
                var service = ConnectedAnimationService.GetForCurrentView();
                var ani = service.GetAnimation("DropAni");
                if (ani != null)
                {
                    if (!ani.TryStart(Artwork, new UIElement[] { NowPanelTexts }))
                    {
                        DropHint.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    DropHint.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            if (DropHint.Visibility == Visibility.Collapsed)
                return;

            dropTimer?.Cancel();
            var service = ConnectedAnimationService.GetForCurrentView();

            //OffsetX Custom Animation
            var yAnimation = Window.Current.Compositor.CreateScalarKeyFrameAnimation();
            yAnimation.Duration = TimeSpan.FromSeconds(1);
            yAnimation.InsertExpressionKeyFrame(0.0f, "StartingValue");
            yAnimation.InsertExpressionKeyFrame(1.0f, "FinalValue", Window.Current.Compositor.CreateCubicBezierEasingFunction(new System.Numerics.Vector2(0.6f, -0.28f), new System.Numerics.Vector2(0.735f, 0.045f)));

            var xAnimation = Window.Current.Compositor.CreateScalarKeyFrameAnimation();
            xAnimation.Duration = TimeSpan.FromSeconds(1);
            xAnimation.InsertExpressionKeyFrame(0.0f, "StartingValue");
            xAnimation.InsertExpressionKeyFrame(1.0f, "FinalValue", Window.Current.Compositor.CreateCubicBezierEasingFunction(new System.Numerics.Vector2(0.47f, 0f), new System.Numerics.Vector2(0.745f, 0.715f)));

            var ani = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("DropAni", DropHint);
            ani.SetAnimationComponent(ConnectedAnimationComponent.OffsetY, yAnimation);
            ani.SetAnimationComponent(ConnectedAnimationComponent.OffsetX, xAnimation);
            ani.SetAnimationComponent(ConnectedAnimationComponent.CrossFade, xAnimation);
            ani.SetAnimationComponent(ConnectedAnimationComponent.Scale, xAnimation);
            ani.Completed += (a, s) =>
            {
                DropHint.Visibility = Visibility.Collapsed;
            };
        }

        private void VolumeFlyout_Open(object sender, object e)
        {
            Context.Volume = Settings.Current.PlayerVolume;
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Context.PositionChange(Context.TotalDuration * (e.NewValue / 100d));
        }

        private async void Delete_SearchHistory(object sender, RoutedEventArgs e)
        {
            var g = (sender as Control).DataContext as GenericMusicItemViewModel;
            await SQLOperator.Current().DeleteSearchHistoryAsync(g.Title);
            Context.SearchItems.Remove(g);
        }

        private void NowPlayingFlyout_ItemClick(object sender, ItemClickEventArgs e)
        {
            Context.SkiptoItem(e.ClickedItem as SongViewModel);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if ((Window.Current.Content is Frame f) && f.Content is CompactOverlayPanel) return;


            if (OverlayFrame.Visibility == Visibility.Visible && OverlayFrame.Content is IRequestGoBack g)
            {
                g.RequestGoBack();
                return;
            }
            if (MainFrame.Visibility == Visibility.Visible && MainFrame.Content is IRequestGoBack p)
            {
                p.RequestGoBack();
                return;
            }

            GoBack();
        }

        private void HamPane_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            var index = HamPane.SelectedIndex;
            if (index < 0)
                return;

            if (OverlayFrame.Visibility == Visibility.Visible)
            {
                GoBackFromNowPlaying();
            }

            if (MainFrame.Content is SettingsPage || MainFrame.Content is AboutPage)
            {
                MainFrame.Navigate(Context.HamList[index].TargetType);
                return;
            }

            MainFrame.Navigate((Context.HamList[index] as HamPanelItem).TargetType);
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            HamPane.SelectionChanged -= HamPane_SelectionChanged;
            var index = Context.HamList.FindIndex(a => a.TargetType == MainFrame.Content.GetType());
            HamPane.SelectedIndex = index;
            HamPane.SelectionChanged += HamPane_SelectionChanged;
        }

        private void DimissMoreFlyout(object sender, RoutedEventArgs e)
        {
            MoreFlyout.Hide();
        }

        private void NowPanelButton_ContextRequested(UIElement sender, Windows.UI.Xaml.Input.ContextRequestedEventArgs args)
        {
            var model = Context.CurrentSong;
            if (model == null)
            {
                return;
            }

            var flyout = Resources["NowPlayingFlyout"] as MenuFlyout;

            var requestedElement = (FrameworkElement)args.OriginalSource;

            var albumMenu = flyout.Items.First(x => x.Name == "NowPlayingAlbum") as MenuFlyoutItem;
            albumMenu.Text = model.Album;
            albumMenu.Visibility = Visibility.Visible;

            // remove performers in flyout
            var index = flyout.Items.IndexOf(albumMenu);
            while (!(flyout.Items[index + 1] is MenuFlyoutSeparator))
            {
                flyout.Items.RemoveAt(index + 1);
            }
            // add song's performers to flyout
            if (!model.Performers.IsNullorEmpty())
            {
                if (model.Performers.Length == 1)
                {
                    var menuItem = new MenuFlyoutItem()
                    {
                        Text = $"{model.Performers[0]}",
                        Icon = new FontIcon()
                        {
                            Glyph = "\uE136"
                        }
                    };
                    menuItem.Click += MenuFlyoutArtist_Click;
                    flyout.Items.Insert(index + 1, menuItem);
                }
                else
                {
                    var sub = new MenuFlyoutSubItem()
                    {
                        Text = $"{Consts.Localizer.GetString("PerformersText")}:",
                        Icon = new FontIcon()
                        {
                            Glyph = "\uE136"
                        }
                    };
                    foreach (var item in model.Performers)
                    {
                        var menuItem = new MenuFlyoutItem()
                        {
                            Text = item
                        };
                        menuItem.Click += MenuFlyoutArtist_Click;
                        sub.Items.Add(menuItem);
                    }
                    flyout.Items.Insert(index + 1, sub);
                }


                if (args.TryGetPosition(requestedElement, out var point))
                {
                    flyout.ShowAt(requestedElement, point);
                }
                else
                {
                    flyout.ShowAt(requestedElement);
                }

                args.Handled = true;
            }
        }

        private async void NowPlayingAlbum_Click(object sender, RoutedEventArgs e)
        {
            var song = new SongViewModel(Context.CurrentSong);
            var viewModel = await song.GetAlbumAsync();

            var d = new AlbumViewDialog(viewModel);
            await d.ShowAsync();
        }

        private async void NowPlayingAddCollectioin_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddPlayList(Context.CurrentSong.ID);

            await dialog.ShowAsync();
        }

        private void NowPlayingShare_Click(object sender, RoutedEventArgs e)
        {
            var shareTitle = $"I'm sharing {Context.CurrentTitle} to you";
            var shareDesc = $"{new SongViewModel(Context.CurrentSong).ToString()}";
            DataTransferManager.ShowShareUI();
        }

        private async void NowPlayingTag_Click(object sender, RoutedEventArgs e)
        {
            await new TagDialog(new SongViewModel(Context.CurrentSong)).ShowAsync();
        }

        private async void NowPlayingExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (Context.CurrentSong.IsOnline)
            {
                PopMessage("Online Item");
                return;
            }
            var path = Context.CurrentSong.FilePath;
            if (!path.IsNullorEmpty())
            {
                var file = await StorageFile.GetFileFromPathAsync(path);
                var option = new FolderLauncherOptions();
                option.ItemsToSelect.Add(file);
                await Launcher.LaunchFolderAsync(await file.GetParentAsync(), option);
            }
        }

        private void HideMagic(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            e.Handled = true;
            MagicBorder.Visibility = Visibility.Collapsed;
        }

        private void MagicBorderHide(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            e.Handled = true;
            MagicBorder.Visibility = Visibility.Collapsed;
        }
    }
}
