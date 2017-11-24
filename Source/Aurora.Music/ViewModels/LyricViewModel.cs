using Aurora.Music.Core.Models;
using Aurora.Shared.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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
            return "No Lyrics.";
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
            return "End";
        }

        private Lyric lyric;

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

        public void Update(TimeSpan current)
        {
            lock (Contents)
            {
                if (lyric == null || Contents.Count == 0)
                {
                    return;
                }
                bool b = false;
                for (int i = 0; i < lyric.Count; i++)
                {
                    Contents[i].IsCurrent = false;
                    if (!b && current < (lyric[i].Key + lyric.Offset))
                    {
                        if (i == 0)
                        {
                            CurrentIndex = 0;
                            Contents[0].IsCurrent = true;
                        }
                        else
                        {
                            CurrentIndex = i - 1;
                            Contents[i - 1].IsCurrent = true;
                        }
                        b = true;
                    }
                }
                if (!b)
                {
                    CurrentIndex = lyric.Count - 1;
                    Contents[lyric.Count - 1].IsCurrent = true;
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
    }
}
