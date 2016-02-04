using UnityEngine;
using System;
using System.Collections.Generic;
using Settworks.Hexagons;

[Serializable]
public class Map : ISerializationCallbackReceiver
{
    public int GridWidth = 5;
    public int GridHeight = 5;
    public int FacilityRadius = 5;
    public int PeripheralRadius = 12;

    public string SurfaceHeightMap;

    public float SurfaceHeightScale = 5.0f;
    public float AttenuationMultiplier = 0.0f;
    public float AttenuationExponent = 0.0f;
    public HexMeshAttentuationStyle AttenuationStyle;
    public HexMeshSurfaceStyle SurfaceStyle;

    [Range(0.0f, 1.0f)]
    public float SurfaceStyleInterpolation = 1.0f;

    public bool SurfaceStyleAttenuation = false;
    public HexMeshNeighborStyle NeighborStyle;

    [Range(0.0f, 1.0f)]
    public float NeighborStyleInterpolation = 1.0f;

    [Range(0.0f, 1.0f)]
    public float DetailWidth = 0.2f;

    public Dictionary<HexCoord, float> Offsets;
    public List<MapElevationOffset> ElevationOffsets; // Serializable version of Offsets

    public float GetOffset(HexCoord coord)
    {
        float value = 0.0f;
        if (Offsets != null)
        {
            Offsets.TryGetValue(coord, out value);
        }
        return value;
    }

    public void SetOffset(HexCoord coord, float value)
    {
        if (Offsets == null)
        {
            Offsets = new Dictionary<HexCoord, float>();
        }
        Offsets[coord] = value;
    }

    public void ChangeOffset(HexCoord coord, float change)
    {
        SetOffset(coord, GetOffset(coord) + change);
    }

    public void ChangeOffset(IEnumerable<HexCoord> coords, float change)
    {
        foreach (var coord in coords) {
            SetOffset(coord, GetOffset(coord) + change);
        }
    }

    public void OnBeforeSerialize()
    {
        if (ElevationOffsets == null)
        {
            ElevationOffsets = new List<MapElevationOffset>();
        }
        else
        {
            ElevationOffsets.Clear();
        }

        if (Offsets != null)
        {
            ElevationOffsets.Capacity = Offsets.Count;
            foreach (var item in Offsets)
            {
                ElevationOffsets.Add(new MapElevationOffset(item.Key, item.Value));
            }
        }
    }

    public void OnAfterDeserialize()
    {
        if (Offsets == null)
        {
            Offsets = new Dictionary<HexCoord, float>(ElevationOffsets != null ? ElevationOffsets.Count : 0);
        }
        else
        {
            Offsets.Clear();
        }

        if (ElevationOffsets != null)
        {
            foreach (var item in ElevationOffsets)
            {
                Offsets.Add(item.Coord, item.Offset);
            }
            ElevationOffsets.Clear();
        }
    }

    [Serializable]
    public struct MapElevationOffset
    {
        public HexCoord Coord;
        public float Offset;

        public MapElevationOffset(HexCoord coord, float offset)
        {
            Coord = coord;
            Offset = offset;
        }
    }
}