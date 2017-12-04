using System;
using Aurora.Music.Pages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Aurora.Music.ViewModels;
using Aurora.Shared.Controls;
using Windows.UI.Input;
using Windows.UI.Xaml.Media.Animation;
using Aurora.Music.Core;
using Windows.UI.Xaml.Media;
using Aurora.Shared;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using Windows.System.Threading;
using Aurora.Shared.Extensions;
using Aurora.Music.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using System.Collections.Generic;
using Aurora.Music.Core.Models;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.UI;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Primitives;
using System.Linq;

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

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            MainFrame.Navigate(typeof(HomePage));
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
            }, TimeSpan.FromMilliseconds(1200));
        }

        private int lyricViewID;
        private IAsyncAction searchTask;
        private StackPanel autoSuggestPopupPanel;
        private ThreadPoolTimer dismissTimer;

        internal void GoBack()
        {
            throw new NotImplementedException();
        }

        public bool SubPageCanGoBack { get => MainFrame.Visibility == Visibility.Visible; }
        public bool CanAdd { get; private set; }

        private void Main_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            MainFrame.Navigate(navigateOptions[sender.MenuItems.IndexOf(args.SelectedItem)]);
        }

        string PositionToString(TimeSpan t1, TimeSpan total)
        {
            if (total == null || total == default(TimeSpan))
            {
                return "0:00/0:00";
            }
            return (t1.ToString(@"m\:ss") + '/' + total.ToString(@"m\:ss"));
        }

        public void Navigate(Type type)
        {
            if (OverlayFrame.Visibility == Visibility.Visible)
                return;
            MainFrame.Navigate(type);
        }

        public void Navigate(Type type, object parameter)
        {
            if (OverlayFrame.Visibility == Visibility.Visible)
                return;
            MainFrame.Navigate(type, parameter);
        }

        private void Toggle_PaneOpened(object sender, RoutedEventArgs e) => Root.IsPaneOpen = !Root.IsPaneOpen;

        public SolidColorBrush TitleForeground(bool b) => (SolidColorBrush)(b ? Resources["SystemControlForegroundAltHighBrush"] : Resources["SystemControlForegroundBaseHighBrush"]);

        private void Pane_CurrentChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OverlayFrame.Visibility == Visibility.Visible)
            {
                GoBackFromNowPlaying();
            }

            if (((sender as ListView).SelectedItem as HamPanelItem) == Context.HamList.Find(x => x.IsCurrent))
            {
                Root.IsPaneOpen = false;
                return;
            }

            foreach (var item in Context.HamList)
            {
                item.IsCurrent = false;
            }
            ((sender as ListView).SelectedItem as HamPanelItem).IsCurrent = true;
            MainFrame.Navigate(((sender as ListView).SelectedItem as HamPanelItem).TargetType);
            Root.IsPaneOpen = false;
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

        private void FastFoward_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Canceled)
            {
                Context.FastForward(false);
            }
            else
            {
                Context.FastForward(true);
            }
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
            }
            else
            {
                TitleBar.Visibility = Visibility.Collapsed;
                TitlebarBtm.Visibility = Visibility.Collapsed;
                SearchBox.Margin = new Thickness(0);
            }

        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            if (sender.IsVisible)
            {
                SearchBox.Margin = new Thickness(0, 0, sender.SystemOverlayRightInset, 0);
            }
            else
            {
                SearchBox.Margin = new Thickness(0);
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
                var dialog = new SearchResultDialog(g);
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Secondary)
                {
                    var view = new AlbumViewDialog(await g.FindAssociatedAlbumAsync());
                    result = await view.ShowAsync();
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
                    var dialog = new SearchResultDialog(Context.SearchItems[0]);
                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Secondary)
                    {
                        var view = new AlbumViewDialog(await Context.SearchItems[0].FindAssociatedAlbumAsync());
                        result = await view.ShowAsync();
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
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
            {
                return;
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
            if ((Context.SearchItems != null && Context.SearchItems.Count < 1) || (!Context.SearchItems.IsNullorEmpty() && !Context.SearchItems[0].Title.IsNullorEmpty()))
                lock (Lockable)
                {
                    Context.SearchItems.Clear();
                    for (int i = 0; i < 5; i++)
                    {
                        Context.SearchItems.Add(new GenericMusicItemViewModel());
                    }
                }
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
            e.Handled = true;

            e.DragUIOverride.SetContentFromBitmapImage(new BitmapImage(new Uri(Consts.BlackPlaceholder)));

            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            var p = await e.DataView.GetStorageItemsAsync();
            if (p.Count > 0 && IsSongsFile(p))
            {
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.Caption = "Drop to Play";
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsContentVisible = true;
            }
            else
            {
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.Caption = "Not Support";
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsContentVisible = false;
            }
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
            e.Handled = true;
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            ShowModalUI(true, "Loading Files");
            var p = await e.DataView.GetStorageItemsAsync();
            var list = new List<StorageFile>();
            if (p.Count > 0)
            {
                list.AddRange(await CopyFilesAsync(p));
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
            var songs = await Context.ComingNewSongsAsync(list);

            await Context.InstantPlay(songs);

            if (songs.Count > 0)
            {
                ShowDropSongsUI(songs);
            }
        }

        private async void ShowDropSongsUI(IList<Song> songs)
        {
            ShowModalUI(false);
            var dialog = new DropSongsDialog(songs);
            await dialog.ShowAsync();
        }

        public async Task FileActivated(IReadOnlyList<IStorageItem> p)
        {
            var list = new List<StorageFile>();
            if (p.Count > 0)
            {
                list.AddRange(await CopyFilesAsync(p));
            }
            else
            {
                return;
            }

            ShowModalUI(true, "Loading Files");

            var songs = await Context.ComingNewSongsAsync(list);

            await Context.InstantPlay(songs);

            if (songs.Count > 0)
            {
                ShowDropSongsUI(songs);
            }
        }

        private void ShowModalUI(bool v1, string v2 = "")
        {
            if (v1)
            {
                ModalIn.Begin();
            }
            else
            {
                ModalOut.Begin();
            }
            ModalText.Text = v2;
        }

        private static async Task<IReadOnlyList<StorageFile>> CopyFilesAsync(IReadOnlyList<IStorageItem> p)
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
                            list.Add(await file.CopyAsync(await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("Songs", CreationCollisionOption.OpenIfExists), file.Name, NameCollisionOption.ReplaceExisting));
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

        private async void SearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            var box = sender as AutoSuggestBox;

            var up = box.GetFirst<Popup>();
            autoSuggestPopupPanel = up.Child as StackPanel;
            await Task.Delay(400);
            if (Context.OnlineMusicExtension != null)
            {
                SearchBox.PlaceholderText = "Search in library and web";
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.LayoutMetricsChanged -= CoreTitleBar_LayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged -= CoreTitleBar_IsVisibleChanged;
            GC.Collect();
        }
    }
}
