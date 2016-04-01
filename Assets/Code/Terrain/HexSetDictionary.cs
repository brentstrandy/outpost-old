using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
public class HexSetDictionary<TKey> : OrderedDictionary<TKey, HexSetDictionary<TKey>.Entry> where TKey : struct, IConvertible
{
    [Serializable]
    public class Entry
    {
        public TKey Key;
        public HexSet Membership;

        public Entry(TKey key)
        {
            Key = key;
            Membership = new HexSet();
        }
    }

    public HexSetDictionary() : base(x => x.Key) { }

    new public HexSet this[TKey key]
    {
        get
        {
            Entry entry;
            if (!TryGetValue(key, out entry))
            {
                entry = new Entry(key);
                base[key] = entry;
            }
            return entry.Membership;
        }
        set
        {
            if (value.Count == 0)
            {
                Remove(key);
                return;
            }

            Entry entry;
            if (!TryGetValue(key, out entry))
            {
                entry = new Entry(key);
                base[key] = entry;
            }
            entry.Membership = value;
        }
    }
}

