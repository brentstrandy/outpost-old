using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class OrderedSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<T> Values;

    [NonSerialized]
    private Dictionary<T, int> Lookup;

    [NonSerialized]
    private bool Dirty;

    public OrderedSet()
    {
        Values = new List<T>();
        Dirty = true;
    }

    public OrderedSet(IEnumerable<T> items)
    {
        Values = new List<T>();
        foreach (var item in items)
        {
            Values.Add(item);
        }
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
            Lookup = new Dictionary<T, int>(Values.Count);
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
            Lookup[Values[i]] = i;
        }
    }

    private void MakeReady()
    {
        if (Dirty)
        {
            Build();
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

    public void Add(T item)
    {
        MakeReady();
        int index = Values.Count;
        Lookup.Add(item, index); // Throws an error if the key already exists in the lookup
        Values.Add(item);
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

    public bool Contains(T item)
    {
        MakeReady();
        return Lookup.ContainsKey(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        for (int i = 0; i < Values.Count; i++)
        {
            array[i + arrayIndex] = Values[i];
        }
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        var exception = new HashSet<T>(other);
        if (Values.Count < 24 || exception.Count < 2 || Values.Count * exception.Count < 256)
        {
            foreach (var item in exception)
            {
                Remove(item);
            }
            return;
        }

        var source = PrepareResample();
        foreach (var item in source)
        {
            if (!exception.Contains(item))
            {
                Add(item);
            }
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        var intersection = new HashSet<T>(other);
        if (Values.Count < 24 || intersection.Count < 2 || Values.Count * intersection.Count < 256)
        {
            for (int i = 0; i < Values.Count;)
            {
                var item = Values[i];
                if (intersection.Contains(item))
                {
                    Lookup[item] = i;
                    i++;
                    continue;
                }
                Values.RemoveAt(i);
                if (Lookup != null)
                {
                    Lookup.Remove(item);
                }
            }
            return;
        }

        var source = PrepareResample();
        foreach (var item in source)
        {
            if (intersection.Contains(item))
            {
                Add(item);
            }
        }
    }

    public bool Remove(T item)
    {
        MakeReady();
        int index;
        if (Lookup.TryGetValue(item, out index))
        {
            Values.RemoveAt(index);
            Lookup.Remove(item);
            UpdateLookup(index);
            return true;
        }

        return false;
    }

    public void UnionWith(IEnumerable<T> other)
    {
        MakeReady();
        foreach (var item in other)
        {
            if (!Lookup.ContainsKey(item))
            {
                Add(item);
            }
        }
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

    // Clears the set in preparation for a complete resampling of its values.
    // Effectively just clears and returns the pre-cleared data.
    private List<T> PrepareResample()
    {
        var source = Values;
        Values = new List<T>();
        if (Lookup != null)
        {
            Lookup.Clear();
        }
        else
        {
            Lookup = new Dictionary<T, int>(source.Count);
        }
        Dirty = false;

        return source;
    }
}
