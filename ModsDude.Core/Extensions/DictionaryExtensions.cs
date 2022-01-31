using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core.Utils;

public static class DictionaryExtensions
{
    public static IDictionary<TKey, TValue1> ExceptOnKeys<TKey, TValue1, TValue2>(this IDictionary<TKey, TValue1> first, IDictionary<TKey, TValue2> second) where TKey : IEquatable<TKey>
    {
        return new Dictionary<TKey, TValue1>(first.Where(kv => second.ContainsKey(kv.Key) == false));
    }

    public static IDictionary<TKey, TValue1> IntersectOnKeys<TKey, TValue1, TValue2>(this IDictionary<TKey, TValue1> first, IDictionary<TKey, TValue2> second) where TKey : IEquatable<TKey>
    {
        return new Dictionary<TKey, TValue1>(first.Where(kv => second.ContainsKey(kv.Key)));
    }
}
