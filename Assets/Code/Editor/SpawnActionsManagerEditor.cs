using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnActionsManager))]
public class SpawnActionsManagerEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SpawnActionsManager myScript = (SpawnActionsManager)target;

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Save Data"))
            {
                XMLParser<SpawnAction>.XMLSerializer_List(myScript.container.SpawnAction_List);
            }
        }
    }
}
