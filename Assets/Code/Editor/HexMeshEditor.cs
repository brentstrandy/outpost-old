#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Settworks.Hexagons;

[CustomEditor(typeof(HexMesh))]
public class HexMeshEditor : Editor
{
    public static bool IsEditing = false;
    public static bool HasFocus = false;
    public static float BrushStrength = 0.5f;
    public const float MinBrushStrength = 0.1f;
    public const float MaxBrushStrength = 5.0f;
    public static int BrushRadius = 1;
    public const int MinBrushRadius = 1;
    public const int MaxBrushRadius = 5;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (target.GetType() == typeof(HexMesh))
        {
            HexMesh hm = (HexMesh)target;

            // Trigger the setters
            if (GUILayout.Button("Apply Dimensions"))
            {
                hm.ApplyProperties();
            }

            // Trigger the setters
            IsEditing = GUILayout.Toggle(IsEditing, "Edit Surface", "Button");
            if (IsEditing)
            {
                GUILayout.Label("Brush Strength");
                BrushStrength = GUILayout.HorizontalSlider(BrushStrength, MinBrushStrength, MaxBrushStrength);
                GUILayout.Label("Brush Radius");
                BrushRadius = EditorGUILayout.IntSlider(BrushRadius, MinBrushRadius, MaxBrushRadius);
            }
        }
    }

    public void OnSceneGUI()
    {
        if (target.GetType() == typeof(HexMesh))
        {
            if (Application.isPlaying)
            {
                return;
            }

            HexMesh terrain = (HexMesh)target;

            if (terrain.Overlays == null)
            {
                terrain.CreateOverlays();
            }
            var overlay = terrain.Overlays[(int)TerrainOverlays.Editor][0];

            bool shouldHaveFocus = IsEditing && Event.current.control;
            if (HasFocus && !shouldHaveFocus)
            {
                HasFocus = false;
                GUIUtility.hotControl = 0;
            }

            if (shouldHaveFocus)
            {
                HasFocus = true;
                //HandleUtility.AddDefaultControl(controlID);
                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
            }

            if (IsEditing)
            {
                //HideSelectionWireframe();
            }
            else
            {
                overlay.Hide();
                return;
            }

            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseMove)
            {
                Camera cam = SceneView.currentDrawingSceneView.camera;
                Vector2 pos = Event.current.mousePosition;
                pos.y = cam.pixelHeight - pos.y; // Convert GUI space to screen space by inverted the Y axis
                Ray ray = cam.ScreenPointToRay(pos);
                RaycastHit hit;
                HexCoord coord;

                bool overTerrain = terrain.IntersectRay(ray, out hit, out coord);
                if (overTerrain)
                {
                    if (Event.current.type == EventType.MouseDown && HasFocus)
                    {
                        float offset = Event.current.button == 0 ? -BrushStrength : BrushStrength;

                        if (terrain.Map != null)
                        {
                            // We always change the offsets of BrushRadius - 1, because a user-selected radius of 1 actually
                            // means no expansion beyond the selected hexagon.
                            terrain.Map.ChangeOffset(HexKit.WithinRange(coord, BrushRadius - 1, false), offset);

                            // We always update the mesh for the updated hexagons plus an additional ring of hexagons so that
                            // their transitions can be updated.
                            terrain.UpdateMesh(HexKit.WithinRange(coord, BrushRadius, false));
                            terrain.UpdateOverlays(HexKit.WithinRange(coord, BrushRadius, false));

                            overlay = terrain.Overlays[(int)TerrainOverlays.Editor][0]; // UpdateMesh could recreate the overlays so we grab this again
                        }
                    }

                    overlay.Color = Color.cyan;
                    overlay.IncludeAndUpdate(HexKit.WithinRange(coord, BrushRadius - 1, false));
                    overlay.Show();
                }
                else
                {
                    overlay.Hide();
                }

                if (HasFocus)
                {
                    Event.current.Use();
                }
            }
        }
    }

    private void ShowSelectionWireframe()
    {
        foreach (GameObject s in Selection.gameObjects)
        {
            var rend = s.GetComponent<Renderer>();
            if (rend != null)
            {
                EditorUtility.SetSelectedWireframeHidden(rend, false);
            }
        }

    }

    private void HideSelectionWireframe()
    {
        foreach (GameObject s in Selection.gameObjects)
        {
            var rend = s.GetComponent<Renderer>();
            if (rend != null)
            {
                EditorUtility.SetSelectedWireframeHidden(rend, true);
            }
        }
    }
}

#endif