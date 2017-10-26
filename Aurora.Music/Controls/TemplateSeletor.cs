using Aurora.Music.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Aurora.Music.Controls
{
    class HomePageTemplateSeletor : DataTemplateSelector
    {
        public DataTemplate AlbumTemplate { get; set; }
        public DataTemplate SongTemplate { get; set; }
        

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is AlbumViewModel a)
            {
                return AlbumTemplate;
            }
            if (item is SongViewModel s)
            {
                return SongTemplate;
            }
            return null;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplate(item);
        }
    }
}
