using Aurora.Music.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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

    class SongListTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EvenTemplate { get; set; }
        public DataTemplate OddTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is SongViewModel s)
            {
                if (s.Index % 2 == 0)
                {
                    return EvenTemplate;
                }
                else
                {
                    return OddTemplate;
                }
            }
            return EvenTemplate;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var listview = ItemsControl.ItemsControlFromItemContainer(container);
            var index = listview.IndexFromContainer(container);
            if (index % 2 == 0)
            {
                return EvenTemplate;
            }
            else
            {
                return OddTemplate;
            }
        }
    }
}
