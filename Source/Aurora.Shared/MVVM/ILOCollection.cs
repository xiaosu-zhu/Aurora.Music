// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace Aurora.Shared.MVVM
{
    public abstract class ILOCollection<T> : IList, IList<T>, INotifyCollectionChanged, ISupportIncrementalLoading
    {
        private List<T> collection = new List<T>();

        private Object _syncRoot = new object();

        public object this[int index] { get => collection[index]; set => collection[index] = (T)value; }
        T IList<T>.this[int index] { get => collection[index]; set => collection[index] = value; }

        public int Count => collection.Count;

        public bool HasMoreItems => HasMoreItemsOverride();

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        // Is this List synchronized (thread-safe)?
        bool ICollection.IsSynchronized { get; } = false;
        // Synchronization root for this object.
        Object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                }
                return _syncRoot;
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(T item)
        {
            collection.Add(item);
            //this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public int Add(object value)
        {
            collection.Add((T)value);

            //this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));

            return collection.Count - 1;
        }

        public void Clear()
        {
            collection.Clear();
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {
            return collection.Contains(item);
        }

        public bool Contains(object value)
        {
            return collection.Contains((T)value);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            collection.CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            collection.CopyTo((T[])array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return collection.IndexOf(item);
        }

        public int IndexOf(object value)
        {
            return collection.IndexOf((T)value);
        }

        public void Insert(int index, T item)
        {
            collection.Insert(index, item);

            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void Insert(int index, object value)
        {
            collection.Insert(index, (T)value);

            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            lock (_syncRoot)
                return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
        }

        protected async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            var baseIndex = collection.Count;
            // 加载开始事件
            this.OnLoadMoreStarted?.Invoke(count);
            var items = await LoadMoreItemsOverrideAsync(c, count);
            AddItems(items);
            // 加载完成事件
            this.OnLoadMoreCompleted?.Invoke(items == null ? 0 : items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection[i + baseIndex], i + baseIndex);
                CollectionChanged(this, args);
            }
            return new LoadMoreItemsResult { Count = items == null ? 0 : (uint)items.Count };
        }
        public delegate void LoadMoreStarted(uint count);
        public delegate void LoadMoreCompleted(int count);
        public event LoadMoreStarted OnLoadMoreStarted;
        public event LoadMoreCompleted OnLoadMoreCompleted;
        /// <summary>
        /// 将新项目添加进来，之所以是virtual的是为了方便特殊要求，比如不重复之类的
        /// </summary>
        protected virtual void AddItems(IList<T> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    this.Add(item);
                }
            }
        }
        protected abstract Task<IList<T>> LoadMoreItemsOverrideAsync(CancellationToken c, uint count);
        protected abstract bool HasMoreItemsOverride();

        public bool Remove(T item)
        {
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            return collection.Remove(item);
        }

        public void Remove(object value)
        {
            collection.Remove((T)value);

            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value));
        }

        public void RemoveAt(int index)
        {
            var item = collection[index];
            collection.RemoveAt(index);

            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return collection.GetEnumerator();
        }
    }
}
