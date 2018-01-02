using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.ViewModels
{
    public class CategoryListItem : ViewModelBase
    {
        public string Title { get; set; }

        public Uri Index { get; set; }

        private bool isCurrent;
        public bool IsCurrent
        {
            get { return isCurrent; }
            set { SetProperty(ref isCurrent, value); }
        }

        public Type NavigatType { get; set; }

        public double GetHeight(bool b)
        {
            return b ? 192d : 96d;
        }

    }
}
