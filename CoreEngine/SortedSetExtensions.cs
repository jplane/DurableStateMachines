using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreEngine
{
    internal static class SortedSetExtensions
    {
        public static void Union<T>(this SortedSet<T> set, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                set.Add(item);
            }
        }

        public static bool HasIntersection<T>(this SortedSet<T> set, IEnumerable<T> items)
        {
            if (set.IsEmpty())
            {
                return false;
            }

            return items.Any(item => set.Contains(item));
        }

        public static bool IsEmpty<T>(this SortedSet<T> set)
        {
            return set.Count == 0;
        }
    }
}
