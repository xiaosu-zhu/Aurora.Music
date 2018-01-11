using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Music.Core.Models;
using Windows.Globalization.Collation;
using Aurora.Shared.Extensions;
using System.Diagnostics;
using Aurora.Shared.Helpers;

namespace Aurora.Music.ViewModels
{
    class GroupedItem<T> : IGrouping<string, T>, IEnumerable<T> where T : IKey
    {
        private List<T> list;

        public string Key { get; }

        public override string ToString()
        {
            return Key;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list?.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list?.GetEnumerator();
        }

        public GroupedItem(string key, IEnumerable<T> items)
        {
            list = new List<T>();
            list.AddRange(items);
            Key = key;
        }

        public GroupedItem(IGrouping<string, T> group)
        {
            Key = group.Key;
            list = new List<T>();
            list.AddRange(group);
        }

        public GroupedItem(IGrouping<int, T> group)
        {
            Key = group.Key.ToString("#", CultureInfoHelper.CurrentCulture);
            list = new List<T>();
            list.AddRange(group);
        }

        public GroupedItem(IGrouping<uint, T> group)
        {
            Key = group.Key.ToString("#", CultureInfoHelper.CurrentCulture);
            list = new List<T>();
            list.AddRange(group);
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
            var groups = from i in items group i by RemovePinYin(g.Lookup(i.Key));
            var ordered = groups.OrderBy(z => z.Key, new StringHelper());
            return from p in ordered where !p.IsNullorEmpty() select new GroupedItem<T>(p);
        }

        public static IEnumerable<GroupedItem<T>> CreateGroups(IEnumerable<T> items, Func<T, string> predicate)
        {
            var groups = from i in items group i by predicate(i);
            var ordered = groups.OrderBy(z => z.Key, new StringHelper());
            return from p in ordered where !p.IsNullorEmpty() select new GroupedItem<T>(p);
        }

        public static IEnumerable<GroupedItem<T>> CreateGroups(IEnumerable<T> items, Func<T, int> predicate, bool isDescend = false)
        {
            var groups = from i in items group i by predicate(i);
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
            var groups = from i in items group i by predicate(i);
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
