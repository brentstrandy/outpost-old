using UnityEditor;

[CustomEditor(typeof(TerrainFeature))]
public class TerrainFeatureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (target.GetType() == typeof(TerrainFeature))
        {
            TerrainFeature feature = (TerrainFeature)target;

            feature.TerrainPinning = (TerrainPinningMode)EditorGUILayout.EnumPopup("Terrain Pinning", feature.TerrainPinning);
        }
    }
}