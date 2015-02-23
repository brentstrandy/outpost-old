using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemySpawnActionHandler))]
public class SpawnActionsManagerEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        /*DrawDefaultInspector();
		EnemySpawnActionHandler myScript = (EnemySpawnActionHandler)target;

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Save Data"))
            {
                XMLParser<EnemySpawnAction>.XMLSerializer_List(myScript.container.SpawnAction_List);
            }
        }
        */
    }
}
