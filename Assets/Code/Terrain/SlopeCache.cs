using UnityEngine;
using System;
using System.Collections.Generic;
using Settworks.Hexagons;

public class SlopeCache
{
    protected HexSurface Surface;
    protected float[,,] Cache; // Q x R x N

    protected int Sampling;

    protected HexCoord CacheOffset;
    protected HexCoord CacheSize;

    private readonly HexCoord Zero = new HexCoord();

    public int Count
    {
        get { return CacheSize.q * CacheSize.r; }
    }

    public int SizeInBytes
    {
        get
        {
            return Count * 6 * 4;
        }
    }

    public SlopeCache(HexSurface surface, int sampling)
    {
        Surface = surface;
        Sampling = sampling;
    }

    public void Set(HexCoord coord, int neighbor, float slope)
    {
        coord += CacheOffset;
        if (coord.q < 0 || coord.r < 0 || coord.q >= CacheSize.q || coord.r >= CacheSize.r)
        {
            throw new ArgumentOutOfRangeException("coord", coord, "Cache boundary exceeded in call to Set. Did you forget to call Build() first?");
        }
        neighbor = HexCoord.NormalizeRotationIndex(neighbor);
        Cache[coord.q, coord.r, neighbor] = slope;
    }

    public float Get(HexCoord coord, int neighbor)
    {
        coord += CacheOffset;
        if (coord.q < 0 || coord.r < 0 || coord.q >= CacheSize.q || coord.r >= CacheSize.r)
        {
            return 0.0f;
        }
        neighbor = HexCoord.NormalizeRotationIndex(neighbor);
        return Cache[coord.q, coord.r, neighbor];
    }

    public void Build(IEnumerable<HexCoord> coords)
    {
        HexCoord min, max;
        coords.Bounds(out min, out max);
        var zero = new HexCoord();
        var one = new HexCoord(1, 1);
        CacheOffset = zero - min;
        CacheSize = max - min + one;
        Cache = new float[CacheSize.q, CacheSize.r, 6];
    }

    public void Update(HexCoord coord)
    {
        for (int neighbor = 0; neighbor < 6; neighbor++)
        {
            Update(coord, neighbor);
        }
    }

    public void Update(HexCoord coord, int neighbor)
    {
        Set(coord, neighbor, Calculate(coord, coord.Neighbor(neighbor)));
    }

    /*
    protected Dictionary<SlopeCache.Key, float> Cache;

    public struct Key
    {
        HexCoord From;
        HexCoord To;

        public Key(HexCoord from, HexCoord to)
        {
            From = from;
            To = to;
        }
    }

    public SlopeCache(HexSurface surface, int sampling)
    {
        Surface = surface;
        Cache = new Dictionary<Key, float>();
        Sampling = sampling;
    }

    public void Set(IEnumerable<HexCoord> coords)
    {
        int minQ;
        int minR;
        int maxQ;
        int maxR;
        foreach (var coord in coords)
        {

        }
        HexCoord current;
        current.
    }

    public void Allocate(int q, r)
    {
        int minQ;
        int minR;
        int maxQ;
        int maxR;
        foreach (var coord in coords)
        {

        }
        HexCoord current;
        current.
    }
    */

    /*
    public void Set(HexCoord from, HexCoord to, float slope)
    {
        Key key = new Key(from, to);
        if (Mathf.Approximately(slope, 0.0f))
        {
            Cache.Remove(key);
        }
        else
        {
            Cache[key] = slope;
        }
    }

    public float Get(HexCoord from, HexCoord to)
    {
        float slope;
        Cache.TryGetValue(new Key(from, to), out slope);
        return slope;
    }

    // Update neighbors
    public void Update(HexCoord from)
    {
        foreach (var to in from.Neighbors())
        {
            Update(from, to);
        }
    }

    // Update pair
    public void Update(HexCoord from, HexCoord to)
    {
        Set(from, to, Calculate(from, to));
    }
    */

    private float Calculate(HexCoord from, HexCoord to)
    {
        int steps = HexCoord.Distance(from, to) * Sampling;
        return Surface.Slope(from, to, steps);
    }
}
