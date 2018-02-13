// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Aurora.Music.Controls
{
    class HomePageTemplateSeletor : DataTemplateSelector
    {
        public DataTemplate AlbumTemplate { get; set; }
        public DataTemplate SongTemplate { get; set; }
        public DataTemplate PlayListTemplate { get; set; }
        public DataTemplate PlaceholderTemplate { get; set; }
        public DataTemplate SearchSuggestTemplate { get; set; }
        public DataTemplate PodcastTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is GenericMusicItemViewModel m)
            {
                if (m.Title.IsNullorEmpty())
                {
                    return PlaceholderTemplate;
                }
                switch (m.InnerType)
                {
                    case MediaType.Song:
                        return SongTemplate;
                    case MediaType.Album:
                        return AlbumTemplate;
                    case MediaType.PlayList:
                        return PlayListTemplate;
                    case MediaType.Placeholder:
                        return SearchSuggestTemplate;
                    case MediaType.Podcast:
                        return PodcastTemplate;
                    default:
                        return PlaceholderTemplate;
                }
            }
            return PlaceholderTemplate;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is GenericMusicItemViewModel m)
            {
                if (m.Title.IsNullorEmpty())
                {
                    return PlaceholderTemplate;
                }
                switch (m.InnerType)
                {
                    case MediaType.Song:
                        return SongTemplate;
                    case MediaType.Album:
                        return AlbumTemplate;
                    case MediaType.PlayList:
                        return PlayListTemplate;
                    case MediaType.Placeholder:
                        return SearchSuggestTemplate;
                    case MediaType.Podcast:
                        return PodcastTemplate;
                    default:
                        return PlaceholderTemplate;
                }
            }
            return PlaceholderTemplate;
        }
    }

    class SongListStyleSelector : StyleSelector
    {
        public Style EvenStyle { get; set; }
        public Style OddStyle { get; set; }
        private static ulong i = 0;

        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            if (i++ % 2 == 0)
            {
                return EvenStyle;
            }
            else
            {
                return OddStyle;
            }
        }
    }
}
