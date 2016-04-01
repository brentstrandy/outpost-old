using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    public delegate TKey KeyFunc(TValue value);

    [SerializeField]
    private List<TValue> Values;

    [NonSerialized]
    private KeyFunc KeyDelegate;

    [NonSerialized]
    private Dictionary<TKey, int> Lookup;

    [NonSerialized]
    private bool Dirty;

    public OrderedDictionary(KeyFunc keyDelegate)
    {
        Values = new List<TValue>();
        KeyDelegate = keyDelegate;
        Dirty = true;
    }

    private void Build()
    {
        BuildLookup();

        Dirty = false;
    }

    private void BuildLookup()
    {
        if (Lookup == null)
        {
            Lookup = new Dictionary<TKey, int>(Values.Count);
        }
        else
        {
            Lookup.Clear();
        }

        UpdateLookup(0);
    }

    private void UpdateLookup(int startingIndex)
    {
        for (int i = startingIndex; i < Values.Count; i++)
        {
            Lookup[KeyDelegate(Values[i])] = i;
        }
    }

    private void MakeReady()
    {
        if (Dirty)
        {
            Build();
        }
    }

    public TValue this[TKey key]
    {
        get
        {
            MakeReady();
            return Values[Lookup[key]];
        }

        set
        {
            MakeReady();
            int index;
            if (Lookup.TryGetValue(key, out index))
            {
                Values[index] = value;
            }
            else
            {
                index = Values.Count;
                Values.Add(value);
                Lookup.Add(key, index);
            }
        }
    }

    public int Count
    {
        get
        {
            return Values.Count;
        }
    }

    public bool IsReadOnly
    {
        get
        {
            return false;
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            MakeReady();
            return Lookup.Keys;
        }
    }

    ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
        get
        {
            return Values;
        }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        MakeReady();
        int index = Values.Count;
        Lookup.Add(item.Key, index); // Throws an error if the key already exists in the lookup
        Values.Add(item.Value);
    }

    public void Add(TKey key, TValue value)
    {
        MakeReady();
        int index = Values.Count;
        Lookup.Add(key, index); // Throws an error if the key already exists in the lookup
        Values.Add(value);
    }

    public void Clear()
    {
        Values.Clear();
        if (Lookup != null)
        {
            Lookup.Clear();
        }
        Dirty = false;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        MakeReady();
        int index;
        if (Lookup.TryGetValue(item.Key, out index))
        {
            return EqualityComparer<TValue>.Default.Equals(item.Value, Values[index]);
        }
        return false;
    }

    public bool ContainsKey(TKey key)
    {
        MakeReady();
        return Lookup.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        for (int i = 0; i < Values.Count; i++)
        {
            array[i + arrayIndex] = new KeyValuePair<TKey, TValue>(KeyDelegate(Values[i]), Values[i]);
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new Enumerator(this, 0);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        MakeReady();
        int index;
        if (Lookup.TryGetValue(item.Key, out index) && EqualityComparer<TValue>.Default.Equals(item.Value, Values[index]))
        {
            Values.RemoveAt(index);
            Lookup.Remove(item.Key);
            UpdateLookup(index);
            return true;
        }

        return false;
    }

    public bool Remove(TKey key)
    {
        MakeReady();
        int index;
        if (Lookup.TryGetValue(key, out index))
        {
            Values.RemoveAt(index);
            Lookup.Remove(key);
            UpdateLookup(index);
            return true;
        }

        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        MakeReady();
        int index;
        if (Lookup.TryGetValue(key, out index))
        {
            value = Values[index];
            return true;
        }
        value = default(TValue);
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {
        Lookup = null;
        Dirty = true;
    }

    public struct Enumerator : IEnumerator, IDisposable, IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
    {
        private OrderedDictionary<TKey, TValue> dictionary;
        private int index;

        public Enumerator(OrderedDictionary<TKey, TValue> dict, int idx)
        {
            dictionary = dict;
            index = idx;
        }

        public KeyValuePair<TKey, TValue> Current
        {
            get
            {
                var value = dictionary.Values[index];
                var key = dictionary.KeyDelegate(value);
                return new KeyValuePair<TKey, TValue>(key, value);
            }
        }

        public DictionaryEntry Entry
        {
            get
            {
                var current = Current;
                return new DictionaryEntry(current.Key, current.Value);
            }
        }

        public object Key
        {
            get
            {
                return dictionary.KeyDelegate(dictionary.Values[index]);
            }
        }

        public object Value
        {
            get
            {
                return dictionary.Values[index];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose() { }

        public bool MoveNext()
        {
            if (index + 1 < dictionary.Values.Count)
            {
                index++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            index = 0;
        }
    }
}