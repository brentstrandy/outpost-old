#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using Settworks.Hexagons;

[CustomEditor(typeof(HexTerrain))]
public class HexMeshEditor : Editor
{
    public enum HexMeshEditorMode
    {
        Off,
        EditingSurface,
        EditingDimensions
    }

    public static HexMeshEditorMode Mode = HexMeshEditorMode.Off;

    public bool IsEditingSurface
    {
        get
        {
            return Mode == HexMeshEditorMode.EditingSurface;
        }
        set
        {
            if (value == true)
            {
                Mode = HexMeshEditorMode.EditingSurface;
            }
            else if (Mode == HexMeshEditorMode.EditingSurface)
            {
                Mode = HexMeshEditorMode.Off;
            }
        }
    }

    public bool IsEditingDimensions
    {
        get
        {
            return Mode == HexMeshEditorMode.EditingDimensions;
        }
        set
        {
            if (value == true)
            {
                Mode = HexMeshEditorMode.EditingDimensions;
            }
            else if (Mode == HexMeshEditorMode.EditingDimensions)
            {
                Mode = HexMeshEditorMode.Off;
            }
        }
    }

    public static bool HasFocus = false;
    public static HexMeshBrush Brush = HexMeshBrush.Flat;
    public static float BrushStrength = 0.5f;
    public const float MinBrushStrength = 0.1f;
    public const float MaxBrushStrength = 5.0f;
    public static int BrushRadius = 1;
    public const int MinBrushRadius = 1;
    public const int MaxBrushRadius = 5;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (target.GetType() == typeof(HexTerrain))
        {
            HexTerrain terrain = (HexTerrain)target;

            GUILayout.BeginHorizontal();

            // Trigger the setters
            if (GUILayout.Button("Apply Dimensions"))
            {
                Undo.RecordObject(terrain, "Edit Terrain Dimensions");
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                terrain.ApplyDimensions();
            }

            // Trigger the setters
            if (GUILayout.Button("Bake Height Maps"))
            {
                Undo.RecordObject(terrain, "Bake Height Maps");
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                terrain.BakeHeightMaps();
            }

            GUILayout.EndHorizontal();

            // Trigger the setters
            IsEditingSurface = GUILayout.Toggle(IsEditingSurface, "Edit Surface", "Button");
            if (IsEditingSurface)
            {
                GUILayout.Label("Boundary Brushes");
                Brush = Brush.Toolbar(GUILayout.MinWidth(50f), HexMeshBrush.Inclusion);

                GUILayout.Label("Basic Brushes");
                Brush = Brush.Toolbar(GUILayout.ExpandWidth(false), HexMeshBrush.Flat, HexMeshBrush.Hill, HexMeshBrush.Smoothing, HexMeshBrush.Noise);

                GUILayout.Label("Tilt Brushes");
                Brush = Brush.Toolbar(GUILayout.ExpandWidth(false), HexMeshBrush.TiltAdditive, HexMeshBrush.TiltExclusive);

                //GUILayout.SelectionGrid(0, new string[] { "Flat", "Hill", "Smoothing", "Noise", "Tilt (Additive)" }, 3, GUILayout.MinWidth(200f), GUILayout.ExpandWidth(false));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Brush Strength", GUILayout.Width(100));
                BrushStrength = EditorGUILayout.Slider(BrushStrength, MinBrushStrength, MaxBrushStrength);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Brush Radius", GUILayout.Width(100));
                BrushRadius = EditorGUILayout.IntSlider(BrushRadius, MinBrushRadius, MaxBrushRadius);
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Flatten Everything"))
                {
                    var affected = new List<HexCoord>(terrain.Map.Surface.Keys);
                    terrain.Map.Surface.Clear();
                    terrain.Mesh.Update(affected);
                    terrain.Overlays.Update(affected);
                }
            }

            /*
            IsEditingHexSet = GUILayout.Toggle(IsEditingHexSet, "Edit Hexagon Set", "Button");
            if (IsEditingHexSet)
            {
            }
            */
        }
    }

    public void OnSceneGUI()
    {
        if (target.GetType() == typeof(HexTerrain))
        {
            if (Application.isPlaying)
            {
                return;
            }

            HexTerrain terrain = (HexTerrain)target;

            if (terrain.Overlays == null)
            {
                terrain.CreateOverlays();
            }
            var overlay = terrain.Overlays[(int)TerrainOverlays.Editor][0];

            bool shouldHaveFocus = IsEditingSurface && Event.current.control;
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

            if (IsEditingSurface)
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

                bool overTerrain = terrain.IntersectRay(ray, out hit, out coord) || new HexPlane(terrain.transform.position.z).Intersect(ray, out coord);
                if (overTerrain)
                {
                    if (Event.current.type == EventType.MouseDown && HasFocus)
                    {
                        bool state = Event.current.button == 0;
                        float offset = state ? -BrushStrength : BrushStrength;

                        if (terrain.Map != null)
                        {
                            var surface = terrain.Map.Surface;

                            Undo.RecordObject(terrain, "Edit Terrain");

                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                            // We always change the offsets of BrushRadius - 1, because a user-selected radius of 1 actually
                            // means no expansion beyond the selected hexagon.
                            int range = BrushRadius - 1;
                            var coords = HexKit.WithinRange(coord, range, false);

                            switch (Brush)
                            {
                                case HexMeshBrush.Inclusion:
                                    if (state)
                                    {
                                        terrain.Map.Coords.UnionWith(coords);
                                        terrain.Mesh.Add(coords);
                                    }
                                    else
                                    {
                                        terrain.Map.Coords.ExceptWith(coords);
                                        terrain.Mesh.Remove(coords);
                                    }
                                    break;
                                case HexMeshBrush.Flat:
                                    terrain.Map.Surface.ChangeDistance(coords, offset);
                                    break;
                                case HexMeshBrush.Noise:
                                    foreach (var plot in coords) {
                                        var plane = surface[plot];
                                        var rand = (Vector3)(Random.insideUnitCircle);
                                        rand = Vector3.Normalize(new Vector3(rand.x, rand.y, HexPlane.Up.z));
                                        plane.normal = Vector3.Lerp(plane.normal, rand, 0.05f);
                                        surface[plot] = plane;
                                    }
                                    break;
                                case HexMeshBrush.Smoothing:
                                    foreach (var plot in HexKit.WithinRange(coord, range, false))
                                    {
                                        var plane = surface[plot];
                                        float sampledDistance = surface.Distance(HexKit.WithinRange(plot, 1, false)).Sample(SamplingAlgorithm.Mean);
                                        Vector3 sampledNormal = surface.Normal(HexKit.WithinRange(plot, 1, false)).Sample(SamplingAlgorithm.Mean);
                                        plane.distance += Mathf.Clamp(sampledDistance - plane.distance, -BrushStrength, BrushStrength);
                                        plane.normal = Vector3.Normalize(sampledNormal);
                                        surface[plot] = plane;
                                        //Debug.Log(string.Format("value={0}, mean={1}, diff={2}, change={3}", value, mean, diff, change));
                                    }
                                    break;
                            }

                            // We always update the mesh for the updated hexagons plus an additional ring of hexagons so that
                            // their transitions can be updated.
                            terrain.Mesh.Update(HexKit.WithinRange(coord, BrushRadius, false));
                            terrain.Overlays.Update(HexKit.WithinRange(coord, BrushRadius, false));

                            overlay = terrain.Overlays[(int)TerrainOverlays.Editor][0]; // UpdateMesh could recreate the overlays so we grab this again
                        }
                    }

                    overlay.Color = Color.cyan;
                    overlay.Set(HexKit.WithinRange(coord, BrushRadius - 1, false));
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

    private static void OnUndoRedo()
    {
        var terrain = GameManager.Instance.TerrainMesh;
        if (terrain != null)
        {
            terrain.ApplyDimensions();
        }
    }
}

#endif