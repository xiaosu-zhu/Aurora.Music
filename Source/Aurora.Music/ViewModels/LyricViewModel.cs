// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Shared.MVVM;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Text;

namespace Aurora.Music.ViewModels
{
    class LyricViewModel : ViewModelBase
    {
        private ObservableCollection<LrcContent> content;
        public ObservableCollection<LrcContent> Contents
        {
            get { return content; }
            set { SetProperty(ref content, value); }
        }
        private int index = -1;
        public int CurrentIndex
        {
            get { return index; }
            set { SetProperty(ref index, value); }
        }

        private bool hasLryic;
        public bool HasLyric
        {
            get { return hasLryic; }
            set { SetProperty(ref hasLryic, value); }
        }

        public string GetCurrent(int p)
        {
            if (CurrentIndex < Contents.Count && CurrentIndex > -1)
            {
                return Contents[CurrentIndex].Content;
            }
            return Consts.Localizer.GetString("NoLyricText");
        }


        public string GetPrevious(int p)
        {
            if (CurrentIndex < Contents.Count && CurrentIndex > 0)
            {
                return Contents[CurrentIndex - 1].Content;
            }
            return string.Empty;
        }


        public string GetNext(int p)
        {
            if (CurrentIndex < Contents.Count - 1 && CurrentIndex > -1)
            {
                return Contents[CurrentIndex + 1].Content;
            }
            return Consts.Localizer.GetString("EndText");
        }

        private Lyric lyric;

        public Lyric Lyric { get => lyric; }

        public LyricViewModel()
        {
            Contents = new ObservableCollection<LrcContent>();
        }

        public void Clear()
        {
            lock (Contents)
            {
                CurrentIndex = -1;
                Contents.Clear();
                lyric = null;
                HasLyric = false;
            }
        }

        public void New(Lyric l)
        {
            lock (Contents)
            {
                CurrentIndex = -1;
                Contents.Clear();
                if (l == null || l == default(Lyric))
                {
                    HasLyric = false;
                    return;
                }
                lyric = l;
                foreach (var item in l)
                {
                    Contents.Add(new LrcContent() { Content = item.Value });
                }
                HasLyric = true;
            }
        }

        private int lastIndex = -1;

        public void Update(TimeSpan current)
        {
            lock (Contents)
            {
                var currentIndex = lastIndex;

                if (lyric == null || Contents.Count == 0)
                {
                    return;
                }
                bool b = false;
                for (int i = 0; i < lyric.Count; i++)
                {
                    if (!b && current < (lyric[i].Key + lyric.Offset))
                    {
                        if (i == 0)
                        {
                            currentIndex = 0;
                        }
                        else
                        {
                            currentIndex = i - 1;
                        }
                        b = true;
                    }
                }
                if (!b)
                {
                    currentIndex = lyric.Count - 1;
                }


                if (currentIndex == lastIndex)
                {

                }
                else
                {
                    CurrentIndex = currentIndex;
                    lastIndex = currentIndex;

                    for (int i = 0; i < lyric.Count; i++)
                    {
                        Contents[i].IsCurrent = false;
                        if (i == currentIndex)
                        {
                            Contents[i].IsCurrent = true;
                        }
                    }
                }
            }
        }
    }

    public class LrcContent : ViewModelBase
    {
        public string Content { get; set; }
        private bool isCurrent;
        public bool IsCurrent
        {
            get { return isCurrent; }
            set { SetProperty(ref isCurrent, value); }
        }

        public double CurrentOpacity(bool b)
        {
            return b ? 1d : 0.4d;
        }

        public FontWeight CurrentWeight(bool b)
        {
            return b ? FontWeights.Bold : FontWeights.Normal;
        }
    }
}
