using Aurora.Music.Core.Models;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Music.ViewModels
{
    class LyricViewModel : ViewModelBase
    {
        public List<LrcContent> Contents { get; set; }
        private int index;
        public int CurrentIndex
        {
            get { return index; }
            set { SetProperty(ref index, value); }
        }

        private Lyric lyric;

        public LyricViewModel(Lyric l)
        {
            lyric = l;
            Contents = new List<LrcContent>(l.Select(x => new LrcContent() { Content = x.Value }));
        }

        public void Update(TimeSpan current)
        {
            bool b = false;
            for (int i = 0; i < lyric.Count; i++)
            {
                Contents[i].IsCurrent = false;
                if (!b && current < (lyric[i].Key + lyric.Offset))
                {
                    if (i == 0)
                    {
                        i++;
                    }
                    CurrentIndex = i - 1;
                    Contents[i - 1].IsCurrent = true;
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
