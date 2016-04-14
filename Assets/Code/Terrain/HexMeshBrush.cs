using UnityEngine;
using System.Collections;

public enum HexMeshBrush
{
    Inclusion,
    Passable,
    Buildable,
    Flat,
    Hill,
    TiltAdditive,
    TiltExclusive,
    Smoothing,
    Noise
}

public static class HexMeshBrushExtensions
{
    public static HexMeshBrush Toggle(this HexMeshBrush brush, HexMeshBrush type, string text, GUIStyle style, params GUILayoutOption[] options)
    {
        bool value = GUILayout.Toggle(brush == type, text, style, options);
        return value ? type : brush;
    }

    public static HexMeshBrush Toolbar(this HexMeshBrush brush, GUILayoutOption option, params HexMeshBrush[] types)
    {
        GUIContent[] labels = new GUIContent[types.Length];
        int selection = -1;
        for (int i = 0; i < types.Length; i++)
        {
            labels[i] = new GUIContent(types[i].ToString());
            if (types[i] == brush)
            {
                selection = i;
            }
        }
        selection = GUILayout.Toolbar(selection, labels);
        return selection >= 0 ? types[selection] : brush;
    }

    public static bool IsLayerBrush(this HexMeshBrush brush)
    {
        switch (brush)
        {
            case HexMeshBrush.Passable:
            case HexMeshBrush.Buildable:
                return true;
            default:
                return false;
        }
    }

    public static TerrainLayer Layer(this HexMeshBrush brush)
    {
        switch (brush)
        {
            case HexMeshBrush.Passable:
                return TerrainLayer.Passable;
            case HexMeshBrush.Buildable:
                return TerrainLayer.Buildable;
            default:
                return TerrainLayer.Passable;
        }
    }
}