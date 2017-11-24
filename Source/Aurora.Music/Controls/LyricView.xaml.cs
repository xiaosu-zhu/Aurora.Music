using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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

        public LyricView()
        {
            this.InitializeComponent();
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
            base.OnNavigatedTo(e);
            if (e.Parameter is SongViewModel m)
            {
                Model = m;

                var ext = await Extension.Load(Settings.Load().LyricExtensionID);

                var result = await ext.ExecuteAsync(new KeyValuePair<string, object>("title", model.Title), new KeyValuePair<string, object>("artist", model.Performers.IsNullorEmpty() ? null : model.Performers[0]));
                if (result != null)
                {
                    var l = new Lyric(LrcParser.Parser.Parse((string)result, model.Duration));
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
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            var rootVisual = ElementCompositionPreview.GetElementVisual(BackDrop);
            var _compositor = rootVisual.Compositor;
            var brush = _compositor.CreateHostBackdropBrush();
            var visual = _compositor.CreateSpriteVisual();
            visual.Brush = brush;
            visual.Size = new System.Numerics.Vector2((float)ActualWidth, (float)ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(BackDrop, visual);
            MainPageViewModel.Current.GetPlayer().PositionUpdated += LyricView_PositionUpdated;
            MainPageViewModel.Current.GetPlayer().StatusChanged += LyricView_StatusChanged;
        }

        private async void LyricView_StatusChanged(object sender, PlaybackEngine.StatusChangedArgs e)
        {
            if (e.CurrentSong.ID != Model.ID)
            {
                Model = new SongViewModel(e.CurrentSong);

                var ext = await Extension.Load(Settings.Load().LyricExtensionID);

                var result = await ext.ExecuteAsync(new KeyValuePair<string, object>("title", model.Title), new KeyValuePair<string, object>("artist", model.Performers.IsNullorEmpty() ? null : model.Performers[0]));
                if (result != null)
                {
                    var l = new Lyric(LrcParser.Parser.Parse((string)result, model.Duration));
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
    }
}
