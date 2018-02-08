// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.Pages;
using Aurora.Music.ViewModels;
using Aurora.Shared;
using Aurora.Shared.Controls;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Windows.UI.Xaml.Media;
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
            this.InitializeComponent();

            Current = this;
            MainFrame.Navigate(typeof(HomePage));
            SongFlyout = (Resources["SongFlyout"] as MenuFlyout);

            dataTransferManager = DataTransferManager.GetForCurrentView();
            //Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            //Window.Current.CoreWindow.KeyUp += MainPage_KeyUp;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;

            SystemNavigationManager.GetForCurrentView().BackRequested += MaiPage_BackRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            //SystemNavigationManager.GetForCurrentView().BackRequested -= MaiPage_BackRequested;
            //dataTransferManager.DataRequested -= DataTransferManager_DataRequested;
        }

        private void MaiPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (e.Handled || ((Window.Current.Content is Frame f) && f.Content is CompactOverlayPanel)) return;
            if (MainFrame.Visibility == Visibility.Collapsed && OverlayFrame.Visibility == Visibility.Visible && OverlayFrame.Content is IRequestGoBack g)
            {
                g.RequestGoBack();
                return;
            }
            if (MainFrame.Visibility == Visibility.Visible && OverlayFrame.Visibility == Visibility.Collapsed && MainFrame.Content is IRequestGoBack p)
            {
                p.RequestGoBack();
                return;
            }

            GoBack();

            e.Handled = true;
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.FailWithDisplayText("Not Implement");
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

        internal void GoBack()
        {
            if (OverlayFrame.Visibility == Visibility.Visible)
            {
            }
            else
            {
                if (MainFrame.CanGoBack)
                {
                    MainFrame.GoBack();
                }
                else
                {
                    MainFrame.Navigate(typeof(HomePage));
                }
            }
            RefreshPaneCurrent();
        }

        private void RefreshPaneCurrent()
        {
            foreach (var item in Context.HamList)
            {
                if (item.TargetType == MainFrame.Content.GetType())
                {
                    item.IsCurrent = true;
                }
                else
                {
                    item.IsCurrent = false;
                }
            }
        }

        private Type[] navigateOptions = { typeof(HomePage), typeof(LibraryPage) };

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

        private int lyricViewID;
        private IAsyncAction searchTask;
        private StackPanel autoSuggestPopupPanel;
        private ThreadPoolTimer dismissTimer;
        private string shareTitle;
        private string shareDesc;
        private DateTime sleepTime;
        private SleepAction sleepAction;
        private ThreadPoolTimer sleepTimer;

        public bool CanAdd { get; private set; }
        public bool IsCurrentDouban => MainFrame.Content is DoubanPage;

        string PositionToString(TimeSpan t1, TimeSpan total)
        {
            if (total == null || total == default(TimeSpan))
            {
                return "0:00/0:00";
            }
            return $"{t1.ToString($@"m\{CultureInfoHelper.CurrentCulture.DateTimeFormat.TimeSeparator}ss", CultureInfoHelper.CurrentCulture)}/{total.ToString(@"m\:ss", CultureInfoHelper.CurrentCulture)}";
        }

        string PositionNarrowToString(TimeSpan t1)
        {
            return t1.ToString($@"m\{CultureInfoHelper.CurrentCulture.DateTimeFormat.TimeSeparator}ss", CultureInfoHelper.CurrentCulture);
        }

        public void Navigate(Type type)
        {
            if (OverlayFrame.Visibility == Visibility.Visible)
                return;
            MainFrame.Navigate(type);
            RefreshPaneCurrent();
        }

        public void Navigate(Type type, object parameter)
        {
            if (OverlayFrame.Visibility == Visibility.Visible)
                return;
            MainFrame.Navigate(type, parameter);
            RefreshPaneCurrent();
        }

        private void Toggle_PaneOpened(object sender, RoutedEventArgs e) => Root.IsPaneOpen = !Root.IsPaneOpen;

        public SolidColorBrush TitleForeground(bool b) => (SolidColorBrush)(b ? Resources["SystemControlForegroundAltHighBrush"] : Resources["SystemControlForegroundBaseHighBrush"]);

        public void ChangeTheme()
        {
            if (MainFrame.Content is IChangeTheme iT)
            {
                iT.ChangeTheme();
            }
            var ui = new UISettings();
            Context.IsDarkAccent = Palette.IsDarkColor(ui.GetColorValue(UIColorType.Accent));
        }

        private void StackPanel_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
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
            if (sender is Panel s)
            {
                (s.Resources["PointerOver"] as Storyboard).Begin();
                e.Handled = true;
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
        }

        private void StackPanel_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["PointerOver"] as Storyboard).Begin();
                e.Handled = true;
            }
        }

        internal async void PopMessage(string msg)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                InAppNotify.Content = msg;
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

        private void StackPanel_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["Normal"] as Storyboard).Begin();
                e.Handled = true;

                s.SetValue(RevealBrush.StateProperty, RevealBrushState.Normal);
            }
        }

        private void NavigatePanel_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["PointerPressed"] as Storyboard).Begin();
                e.Handled = true;
                s.SetValue(RevealBrush.StateProperty, RevealBrushState.Pressed);
            }
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            SearchBox.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);
            SearchButton.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);
            TitlebarBtm.Width = coreTitleBar.SystemOverlayRightInset;
            // Update title bar control size as needed to account for system size changes.
            TitleBar.Height = coreTitleBar.Height;
            TitleBarOverlay.Height = coreTitleBar.Height;

            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;

            Window.Current.SetTitleBar(TitleBar);
        }

        internal void RestoreContext()
        {
            GoBackFromNowPlaying();
        }

        internal async Task ShowLyricWindow()
        {
            await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var frame = new Frame();
                lyricViewID = ApplicationView.GetForCurrentView().Id;
                frame.Navigate(typeof(LyricView), Context.NowPlayingList[Context.CurrentIndex]);
                Window.Current.Content = frame;
                Window.Current.Activate();
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x33, 0x00, 0x00, 0x00);
                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonInactiveForegroundColor = Colors.Gray;
            });
            ViewModePreferences compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            compactOptions.CustomSize = new Size(1000, 100);
            compactOptions.ViewSizePreference = ViewSizePreference.Custom;
            bool viewShown = await ApplicationViewSwitcher.TryShowAsViewModeAsync(lyricViewID, ApplicationViewMode.CompactOverlay, compactOptions);
        }

        internal void HideAutoSuggestPopup()
        {
            autoSuggestPopupPanel.Children[0].Visibility = Visibility.Collapsed;
            ((autoSuggestPopupPanel.Children[0] as Panel).Children[0] as ProgressRing).IsActive = false;
        }

        internal async Task GotoComapctOverlay()
        {
            if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay))
            {
                (Window.Current.Content as Frame).Content = null;

                (Window.Current.Content as Frame).Navigate(typeof(CompactOverlayPanel), Context.NowPlayingList[Context.CurrentIndex]);
            }
        }

        internal void ShowAutoSuggestPopup()
        {
            autoSuggestPopupPanel.Children[0].Visibility = Visibility.Visible;
            ((autoSuggestPopupPanel.Children[0] as Panel).Children[0] as ProgressRing).IsActive = true;
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
            {
                TitleBar.Visibility = Visibility.Visible;
                TitlebarBtm.Visibility = Visibility.Visible;
                SearchBox.Margin = new Thickness(0, 0, sender.SystemOverlayRightInset, 0);
                SearchButton.Margin = new Thickness(0, 0, sender.SystemOverlayRightInset, 0);
            }
            else
            {
                TitleBar.Visibility = Visibility.Collapsed;
                TitlebarBtm.Visibility = Visibility.Collapsed;
                SearchBox.Margin = new Thickness(0);
                SearchButton.Margin = new Thickness(0);
            }

        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            if (sender.IsVisible)
            {
                SearchBox.Margin = new Thickness(0, 0, sender.SystemOverlayRightInset, 0);
                SearchButton.Margin = new Thickness(0, 0, sender.SystemOverlayRightInset, 0);
            }
            else
            {
                SearchBox.Margin = new Thickness(0);
                SearchButton.Margin = new Thickness(0);
            }
            // Update title bar control size as needed to account for system size changes.
            TitlebarBtm.Width = sender.SystemOverlayRightInset;
            TitleBar.Height = sender.Height;
            TitleBarOverlay.Height = sender.Height;
        }

        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
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
                if (g.InnerType == MediaType.Album)
                {
                    var view = new AlbumViewDialog(await g.FindAssociatedAlbumAsync());
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
                        ShowModalUI(true, "Prepare to Show");
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
                            ShowModalUI(true, "Prepare to Show");
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

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
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
            CanAdd = false;
            searchTask?.Cancel();

            var text = sender.Text;

            text = text.Replace('\'', ' ');

            autoSuggestPopupPanel.Children[0].Visibility = Visibility.Visible;
            ((autoSuggestPopupPanel.Children[0] as Panel).Children[0] as ProgressRing).IsActive = true;
            searchTask = ThreadPool.RunAsync(async x =>
            {
                CanAdd = true;
                await Context.Search(text);
            });
        }

        private void Grid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["PointerOver"] as Storyboard).Begin();
            }
        }

        private void Grid_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["Normal"] as Storyboard).Begin();
            }
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            Context.SkiptoItem((sender as Button).DataContext as SongViewModel);
        }

        private async void Root_DragOver(object sender, DragEventArgs e)
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
            }
            else
            {
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.Caption = "Not Support";
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
            d.Complete();
        }

        private async Task FileActivation(IReadOnlyList<IStorageItem> p)
        {
            ShowModalUI(true, "Loading Files");

            var list = new List<StorageFile>();
            if (p.Count > 0)
            {
                list.AddRange(await ReadFilesAsync(p));
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

            await Context.InstantPlay(list);


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

        public static async Task<IReadOnlyList<StorageFile>> ReadFilesAsync(IReadOnlyList<IStorageItem> p)
        {
            var list = new List<StorageFile>();
            foreach (var item in p)
            {
                if (item is IStorageFile file)
                {
                    foreach (var types in Consts.FileTypes)
                    {
                        if (types == file.FileType)
                        {
                            list.Add(file as StorageFile);
                            break;
                        }
                    }
                }
                else if (item is StorageFolder folder)
                {
                    var options = new Windows.Storage.Search.QueryOptions
                    {
                        FileTypeFilter = { ".flac", ".wav", ".m4a", ".aac", ".mp3", ".wma" },
                        FolderDepth = Windows.Storage.Search.FolderDepth.Deep,
                        IndexerOption = Windows.Storage.Search.IndexerOption.DoNotUseIndexer,
                    };
                    var query = folder.CreateFileQueryWithOptions(options);
                    list.AddRange(await query.GetFilesAsync());
                }
            }
            return list;
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

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            switch (SearchButton.Visibility)
            {
                case Visibility.Collapsed:
                    SearchBoxCollapse.Begin();
                    return;
                default:
                    break;
            }

            SearchBoxShow.Begin();
            SearchBox.Focus(FocusState.Programmatic);

            if (SearchBox.Text.IsNullorEmpty())
            {
                Context.SearchItems.Clear();

                // add clipboard text
                DataPackageView dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    string text = await dataPackageView.GetTextAsync();
                    if (!string.IsNullOrWhiteSpace(text))
                        Context.SearchItems.Add(new GenericMusicItemViewModel()
                        {
                            Title = text,
                            InnerType = MediaType.Placeholder,
                            Description = "\uE16D",
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
            if (args.NewFocusedElement is ListViewItem && !Context.SearchItems.IsNullorEmpty())
            {
                if (Context.SearchItems[0].InnerType == MediaType.Placeholder)
                {
                    args.Cancel = true;
                    return;
                }
            }
            SearchBoxCollapse.Begin();
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

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (OverlayFrame.Visibility == Visibility.Visible)
            {
                GoBackFromNowPlaying();
            }

            if (MainFrame.Content is SettingsPage || MainFrame.Content is AboutPage)
            {
                MainFrame.Navigate((e.ClickedItem as HamPanelItem).TargetType);
                Root.IsPaneOpen = false;
                RefreshPaneCurrent();
                return;
            }

            if ((e.ClickedItem as HamPanelItem) == Context.HamList.Find(x => x.IsCurrent))
            {
                Root.IsPaneOpen = false;
                RefreshPaneCurrent();
                return;
            }
            MainFrame.Navigate((e.ClickedItem as HamPanelItem).TargetType);
            Root.IsPaneOpen = false;


            RefreshPaneCurrent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Root.IsPaneOpen = false;
        }

        private async void MenuFlyoutPlay_Click(object sender, RoutedEventArgs e)
        {
            if (SongFlyout.Target is SelectorItem s)
            {
                switch (s.Content)
                {
                    case GenericMusicItemViewModel g:
                        await Context.InstantPlay(await g.GetSongsAsync());
                        break;
                    case SongViewModel song:
                        await Context.InstantPlay(new List<Song>() { song.Song });
                        break;
                    case AlbumViewModel album:
                        await Context.InstantPlay(await album.GetSongsAsync());
                        break;
                    case ArtistViewModel artist:
                        await Context.InstantPlay(await artist.GetSongsAsync());
                        break;

                    default:
                        break;
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
                        await Context.PlayNext(await g.GetSongsAsync());
                        break;
                    case SongViewModel song:
                        await Context.PlayNext(new List<Song>() { song.Song });
                        break;
                    case AlbumViewModel album:
                        await Context.PlayNext(await album.GetSongsAsync());
                        break;
                    case ArtistViewModel artist:
                        await Context.PlayNext(await artist.GetSongsAsync());
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
                        shareDesc = $"I'm sharing {g.Title} to you";
                        break;
                    case SongViewModel song:
                        shareTitle = $"I'm sharing {song.Title} to you";
                        shareDesc = $"I'm sharing {song.Title} to you";
                        break;
                    case AlbumViewModel album:
                        shareTitle = $"I'm sharing {album.Name} to you";
                        shareDesc = $"I'm sharing {album.Name} to you";
                        break;
                    case ArtistViewModel artist:
                        shareTitle = $"I'm sharing {artist.Name} to you";
                        shareDesc = $"I'm sharing {artist.Name} to you";
                        break;

                    default:
                        break;
                }
            }
            DataTransferManager.ShowShareUI();
        }
        private void MenuFlyoutModify_Click(object sender, RoutedEventArgs e)
        {

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
                List<string> paths = new List<string>();
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
        }
        private async void MenuFlyoutTrash_Click(object sender, RoutedEventArgs e)
        {

            if (SongFlyout.Target is SelectorItem s)
            {
                List<string> paths = new List<string>();
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
    }
}
