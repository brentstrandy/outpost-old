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
                //GUILayout.Button("+");
                //GUILayout.Button("-");
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
                            terrain.Map.ChangeOffset(coord, offset);
                            terrain.UpdateMesh(HexKit.WithinRange(coord, 1, false));
                            overlay = terrain.Overlays[(int)TerrainOverlays.Editor][0]; // ApplyProperties can recreate the overlays so we grab this again
                        }
                    }

                    overlay.Update(coord);
                    overlay.Color = Color.cyan;
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