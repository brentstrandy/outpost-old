using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexMesh))]
public class HexMeshInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (target.GetType() == typeof(HexMesh))
        {
            HexMesh hm = (HexMesh)target;

            // Trigger the setters
            if (GUILayout.Button("Apply Dimensions"))
            {
                hm.ApplyProperties();
            }
        }
    }
}