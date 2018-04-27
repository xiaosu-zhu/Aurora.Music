using System.Collections.ObjectModel;

namespace Aurora.Shared.MVVM
{
    public interface IPreloadable<T> where T : class, new()
    {
        bool Preloaded { get; }

        void LoadWithActual(T item);
    }

    public class LazyLoadCollection<T> : ObservableCollection<T> where T : ViewModelBase, IPreloadable<T>, new()
    {
        public int preloadCount = 5;

        private int actualCount = 0;

        public new int Count => actualCount;

        public LazyLoadCollection()
        {
        }

        // shouldn't use blablabla() : this()
        public LazyLoadCollection(int preloadCount)
        {
            this.preloadCount = preloadCount;
        }

        public void Prepare()
        {
            for (int i = 0; i < preloadCount; i++)
            {
                Add(new T());
            }
        }

        public new void Add(T item)
        {
            if (actualCount < preloadCount && base.Count == preloadCount)
            {
                foreach (var i in this)
                {
                    if (i.Preloaded)
                    {
                        continue;
                    }
                    i.LoadWithActual(item);
                    break;
                }
            }
            else
            {
                Add(item);
            }

            actualCount += 1;
        }
    }
}
