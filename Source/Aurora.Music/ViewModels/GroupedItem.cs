// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Globalization.Collation;

namespace Aurora.Music.ViewModels
{
    public class GroupedItem<T> : ObservableCollection<T>, IGrouping<string, T> where T : IKey
    {
        public string Key { get; }

        public override string ToString()
        {
            if (Key.IsNullorEmpty())
            {
                return "?";
            }
            return Key;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public GroupedItem()
        {
        }

        public GroupedItem(string key, IEnumerable<T> items) : base(items)
        {
            Key = key;
        }

        public GroupedItem(IGrouping<string, T> group) : base(group)
        {
            Key = group.Key;
        }

        public GroupedItem(IGrouping<int, T> group) : base(group)
        {
            Key = group.Key.ToString("#", CultureInfoHelper.CurrentCulture);
        }

        public GroupedItem(IGrouping<uint, T> group) : base(group)
        {
            Key = group.Key.ToString("#", CultureInfoHelper.CurrentCulture);
        }

        /// <summary>
        /// Create a list of AlphaGroup<T> with keys set by a SortedLocaleGrouping.
        /// </summary>
        /// <param name="items">The items to place in the groups.</param>
        /// <param name="ci">The CultureInfo to group and sort by.</param>
        /// <param name="getKey">A delegate to get the key from an item.</param>
        /// <param name="sort">Will sort the data if true.</param>
        /// <returns>An items source for a LongListSelector</returns>
        public static IEnumerable<GroupedItem<T>> CreateGroupsByAlpha(IEnumerable<T> items)
        {
            var g = new CharacterGroupings("zh-CN");
            var groups = from i in items orderby i.Key group i by RemovePinYin(g.Lookup(i.Key));
            var ordered = groups.OrderBy(z => z.Key, new StringHelper());
            return from p in ordered where !p.IsNullorEmpty() select new GroupedItem<T>(p);
        }

        public static IEnumerable<GroupedItem<T>> CreateGroups(IEnumerable<T> items, Func<T, string> predicate)
        {
            var groups = from i in items orderby i.Key group i by predicate(i);
            var ordered = groups.OrderBy(z => z.Key, new StringHelper());
            return from p in ordered where !p.IsNullorEmpty() select new GroupedItem<T>(p);
        }

        public static IEnumerable<GroupedItem<T>> CreateGroups(IEnumerable<T> items, Func<T, int> predicate, bool isDescend = false)
        {
            var groups = from i in items orderby i.Key group i by predicate(i);
            if (isDescend)
            {
                var ordered = groups.OrderByDescending(z => z.Key);
                return from p in ordered where !p.IsNullorEmpty() select new GroupedItem<T>(p);
            }
            else
            {
                var ordered = groups.OrderBy(z => z.Key);
                return from p in ordered where !p.IsNullorEmpty() select new GroupedItem<T>(p);
            }
        }

        public static IEnumerable<GroupedItem<T>> CreateGroups(IEnumerable<T> items, Func<T, uint> predicate, bool isDescend = false)
        {
            var groups = from i in items orderby i.Key group i by predicate(i);
            if (isDescend)
            {
                var ordered = groups.OrderByDescending(z => z.Key);
                return from p in ordered where !p.IsNullorEmpty() select new GroupedItem<T>(p);
            }
            else
            {
                var ordered = groups.OrderBy(z => z.Key);
                return from p in ordered where !p.IsNullorEmpty() select new GroupedItem<T>(p);
            }
        }

        private static string RemovePinYin(string v)
        {
            var a = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Where(x => v.Contains(x)).FirstOrDefault();
            if (a != default(char))
            {
                return a.ToString();
            }
            return v;
        }

        private static IEnumerable<string> GetDefaultGroup(CharacterGroupings g)
        {
            return from i in g where !i.Label.IsNullorEmpty() select i.Label;
        }
    }
}
