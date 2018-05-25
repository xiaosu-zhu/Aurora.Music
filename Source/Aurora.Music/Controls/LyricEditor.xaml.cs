using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;

using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Controls
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LyricEditor : Page, IDisposable
    {
        public static LyricEditor Current;

        public static event EventHandler<Lyric> LyricModified;

        public LyricEditor()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
            Current = this;
            autoEvent = new AutoResetEvent(false);
            ApplicationView.GetForCurrentView().Consolidated += LyricEditor_Consolidated;
            RequestedTheme = Settings.Current.Theme;
        }

        private void LyricEditor_Consolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
        {
            ApplicationView.GetForCurrentView().Consolidated -= LyricEditor_Consolidated;
            PlaybackEngine.PlaybackEngine.Current.PositionUpdated -= Current_PositionUpdated;

            LyricModified?.Invoke(null, Model);

            var previous = StampCanvas.Children.Where(a => a is Thumb).ToList();
            foreach (var item in previous)
            {
                StampCanvas.Children.Remove(item);
                (item as Thumb).DragStarted -= T_DragStarted;
                (item as Thumb).DragCompleted -= T_DragCompleted;
                (item as Thumb).DragDelta -= T_DragDelta;
            }
            previous.Clear();
            previous = null;

            Dispose();
            Current = null;

            Window.Current.Close();
        }

        private Lyric model;
        internal Lyric Model
        {
            get => model;
            set
            {
                model = value;
                Task.Run(() => PrepareStamps());
            }
        }

        AutoResetEvent autoEvent;

        private async void PrepareStamps()
        {
            if (model == null)
                return;

            autoEvent.WaitOne();
            await Task.Delay(200);

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Offset.Text = Model.Offset.TotalMilliseconds >= 0 ? $"+ {Model.Offset.ToString(@"s\.ff\s")}" : $"- {Model.Offset.ToString(@"s\.ff\s")}";
                var previous = StampCanvas.Children.Where(a => a is Thumb).ToList();
                foreach (var item in previous)
                {
                    StampCanvas.Children.Remove(item);
                    (item as Thumb).DragStarted -= T_DragStarted;
                    (item as Thumb).DragCompleted -= T_DragCompleted;
                    (item as Thumb).DragDelta -= T_DragDelta;
                }
                previous.Clear();
                previous = null;
                for (int i = 0; i < Model.Count; i++)
                {
                    var item = Model[i];
                    var t = new Thumb()
                    {
                        Style = Resources["ThumbStyle"] as Style,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        RenderTransform = new TranslateTransform()
                        {
                            X = (item.Key + Model.Offset) / TotalPosition * TimeLine.ActualWidth
                        },
                        Tag = i,
                    };
                    t.SetValue(Grid.RowProperty, 1);
                    t.DragStarted += T_DragStarted;
                    t.DragCompleted += T_DragCompleted;
                    t.DragDelta += T_DragDelta;
                    ToolTipService.SetToolTip(t, item.Value);
                    StampCanvas.Children.Add(t);
                }
            });
        }

        private void T_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            (thumb.RenderTransform as TranslateTransform).X += e.HorizontalChange;
            ModifyTimeTranlate.X = (thumb.RenderTransform as TranslateTransform).X;
            ModifyTime.Text = (TotalPosition * (thumb.RenderTransform as TranslateTransform).X / TimeLine.ActualWidth).ToString(@"m\:ss\.ff");
        }

        private void T_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var t = (sender as Thumb);
            var index = (int)t.Tag;
            var percent = TotalPosition * (t.RenderTransform as TranslateTransform).X / TimeLine.ActualWidth;
            Model[index] = new KeyValuePair<TimeSpan, string>(percent, Model[index].Value);

            // update index
            var thumbs = StampCanvas.Children.Where(a => a is Thumb).ToList();
            thumbs.Sort((a, b) =>
            {
                return (a.RenderTransform as TranslateTransform).X.CompareTo((b.RenderTransform as TranslateTransform).X);
            });

            Model.Sort((a, b) => a.Key.CompareTo(b.Key));

            for (int i = 0; i < Model.Count; i++)
            {
                (thumbs[i] as Thumb).Tag = i;
                ToolTipService.SetToolTip(thumbs[i], Model[i].Value);
            }

            ModifyTime.Text = string.Empty;
        }

        private void T_DragStarted(object sender, DragStartedEventArgs e)
        {
        }

        public TimeSpan CurrentPosition { get; private set; }
        public TimeSpan TotalPosition { get; private set; }

        private string filePath;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ValueTuple<LyricViewModel, TimeSpan, string> l)
            {
                Model = l.Item1.Lyric;
                TotalPosition = l.Item2;
                filePath = l.Item3;
            }
            else
            {
                throw new ArgumentException("Lyric is null");
            }

            PlaybackEngine.PlaybackEngine.Current.PositionUpdated += Current_PositionUpdated;
        }

        public async void ChangeLyric(Lyric l)
        {
            Model = l;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                if (StampCanvas != null)
                {
                    autoEvent.Set();
                }
            });
        }

        private void Current_PositionUpdated(object sender, PlaybackEngine.PositionUpdatedArgs e)
        {
            CurrentPosition = e.Current;
            TotalPosition = e.Total;
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                var previous = PreviousLyric.Text;
                var preStamp = PreviousLyric.Header;
                var current = CurrentLyric.Text;
                var currentStamp = CurrentLyric.Header;
                var next = NextLyric.Text;
                var nextStamp = NextLyric.Header;

                var flag = false;
                int i = 0;

                for (; i < Model.Count; i++)
                {
                    if ((Model[i].Key + Model.Offset) >= CurrentPosition)
                    {
                        i -= 1;
                        if (i < 0)
                        {
                            break;
                        }
                        current = Model[i].Value;
                        currentStamp = Model[i].Key.ToString(@"m\:ss\.ff");
                        if (i > 0)
                        {
                            previous = Model[i - 1].Value;
                            preStamp = Model[i - 1].Key.ToString(@"m\:ss\.ff");
                        }
                        if (i < Model.Count - 1)
                        {
                            next = Model[i + 1].Value;
                            nextStamp = Model[i + 1].Key.ToString(@"m\:ss\.ff");
                        }
                        flag = true;
                        break;
                    }
                }
                NowTime.Text = CurrentPosition.ToString(@"m\:ss\.ff");
                TotalTime.Text = TotalPosition.ToString(@"m\:ss\.ff");
                CursorTranslate.X = CurrentPosition / TotalPosition * TimeLine.ActualWidth;
                if (flag)
                {
                    PreviousLyric.Text = previous;
                    PreviousLyric.Header = preStamp;
                    PreviousLyric.Tag = i - 1;

                    CurrentLyric.Text = current;
                    CurrentLyric.Header = currentStamp;
                    CurrentLyric.Tag = i;

                    NextLyric.Text = next;
                    NextLyric.Header = nextStamp;
                    NextLyric.Tag = i + 1;
                }
            });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        }

        private async void StampCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000);
            autoEvent.Set();
        }

        public void Dispose()
        {
            autoEvent.Dispose();
        }

        private void TimeLine_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var before = e.PreviousSize.Width;
            var after = e.NewSize.Width;
            var thumbs = StampCanvas.Children.Where(a => a is Thumb).ToList();
            foreach (var item in thumbs)
            {
                var t = (item.RenderTransform as TranslateTransform);
                t.X = after * t.X / before;
            }
            CursorTranslate.X = CurrentPosition / TotalPosition * TimeLine.ActualWidth;
        }

        private void PreviousLyric_LostFocus(object sender, RoutedEventArgs e)
        {
            var i = (int)PreviousLyric.Tag;
            if (i < Model.Count && i >= 0)
            {
                Model[i] = new KeyValuePair<TimeSpan, string>(Model[i].Key, PreviousLyric.Text);
            }
        }

        private void CurrentLyric_LostFocus(object sender, RoutedEventArgs e)
        {
            var i = (int)CurrentLyric.Tag;
            if (i < Model.Count && i >= 0)
            {
                Model[i] = new KeyValuePair<TimeSpan, string>(Model[i].Key, CurrentLyric.Text);
            }
        }

        private void NextLyric_LostFocus(object sender, RoutedEventArgs e)
        {
            var i = (int)NextLyric.Tag;
            if (i < Model.Count && i >= 0)
            {
                Model[i] = new KeyValuePair<TimeSpan, string>(Model[i].Key, NextLyric.Text);
            }
        }

        private void Offset_LostFocus(object sender, RoutedEventArgs e)
        {
            var txt = Offset.Text;
            txt = string.Join(string.Empty, txt.Where(a => a != ' '));
            var changed = false;
            if (txt.EndsWith("ms", StringComparison.InvariantCultureIgnoreCase))
            {
                txt = txt.Substring(0, txt.Length - 2);
                if (double.TryParse(txt, out var off))
                {
                    Model.Offset = TimeSpan.FromMilliseconds(off);
                    changed = true;
                }
                else
                {

                }
            }
            else if (txt.EndsWith("s", StringComparison.InvariantCultureIgnoreCase))
            {
                txt = txt.Substring(0, txt.Length - 1);
                if (double.TryParse(txt, out var off))
                {
                    Model.Offset = TimeSpan.FromSeconds(off);
                    changed = true;
                }
                else
                {

                }
            }
            else if (double.TryParse(txt, out var off))
            {
                Model.Offset = TimeSpan.FromMilliseconds(off);
                changed = true;
            }
            else
            {

            }
            if (changed)
            {
                UpdateThumbPosition();
            }
        }

        private void UpdateThumbPosition()
        {
            var thumbs = StampCanvas.Children.Where(a => a is Thumb).ToList();
            foreach (var item in thumbs)
            {
                var i = (int)(item as Thumb).Tag;
                var t = (item.RenderTransform as TranslateTransform);
                t.X = (Model[i].Key + Model.Offset) / TotalPosition * TimeLine.ActualWidth;
            }
        }

        private void Little_Backward(object sender, RoutedEventArgs e)
        {
            Model.Offset -= TimeSpan.FromMilliseconds(100);
            UpdateThumbPosition();
            Offset.Text = Model.Offset.TotalMilliseconds >= 0 ? $"+ {Model.Offset.ToString(@"s\.ff\s")}" : $"- {Model.Offset.ToString(@"s\.ff\s")}";
        }

        private void Large_Backward(object sender, RoutedEventArgs e)
        {
            Model.Offset -= TimeSpan.FromMilliseconds(500);
            UpdateThumbPosition();
            Offset.Text = Model.Offset.TotalMilliseconds >= 0 ? $"+ {Model.Offset.ToString(@"s\.ff\s")}" : $"- {Model.Offset.ToString(@"s\.ff\s")}";
        }

        private void Little_Forward(object sender, RoutedEventArgs e)
        {
            Model.Offset += TimeSpan.FromMilliseconds(100);
            UpdateThumbPosition();
            Offset.Text = Model.Offset.TotalMilliseconds >= 0 ? $"+ {Model.Offset.ToString(@"s\.ff\s")}" : $"- {Model.Offset.ToString(@"s\.ff\s")}";
        }

        private void Large_Forward(object sender, RoutedEventArgs e)
        {
            Model.Offset += TimeSpan.FromMilliseconds(500);
            UpdateThumbPosition();
            Offset.Text = Model.Offset.TotalMilliseconds >= 0 ? $"+ {Model.Offset.ToString(@"s\.ff\s")}" : $"- {Model.Offset.ToString(@"s\.ff\s")}";
        }

        private void Offset_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                LoseFocus(sender);
            }
        }

        /// <summary>
        /// Makes virtual keyboard disappear
        /// </summary>
        /// <param name="sender"></param>
        private void LoseFocus(object sender)
        {
            var control = sender as Control;
            var isTabStop = control.IsTabStop;
            control.IsTabStop = false;
            control.IsEnabled = false;
            control.IsEnabled = true;
            control.IsTabStop = isTabStop;
        }

        private void Show_Lrc(object sender, RoutedEventArgs e)
        {
            LrcText.Text = Model;
        }

        private async void Save_File(object sender, RoutedEventArgs e)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(filePath);
                var folder = await file.GetParentAsync();
                var result = await folder.CreateFileAsync($"{file.DisplayName}.lrc", CreationCollisionOption.GenerateUniqueName);
                await FileIO.WriteTextAsync(result, Model);

                var options = new FolderLauncherOptions();
                options.ItemsToSelect.Add(result);
                await Launcher.LaunchFolderAsync(folder, options);
            }
            catch (IOException)
            {
                var folder = KnownFolders.MusicLibrary;
                var result = await folder.CreateFileAsync($"result.lrc", CreationCollisionOption.GenerateUniqueName);
                await FileIO.WriteTextAsync(result, Model);

                var options = new FolderLauncherOptions();
                options.ItemsToSelect.Add(result);
                await Launcher.LaunchFolderAsync(folder, options);
            }
            catch (UnauthorizedAccessException)
            {
                var folder = KnownFolders.MusicLibrary;
                var result = await folder.CreateFileAsync($"result.lrc", CreationCollisionOption.GenerateUniqueName);
                await FileIO.WriteTextAsync(result, Model);

                var options = new FolderLauncherOptions();
                options.ItemsToSelect.Add(result);
                await Launcher.LaunchFolderAsync(folder, options);
            }
            catch (Exception ex)
            {
                // Create the message dialog and set its content
                var messageDialog = new MessageDialog("Error while writing")
                {
                    Content = ex.Message
                };
                // Show the message dialog
                await messageDialog.ShowAsync();
            }
        }
    }
}
