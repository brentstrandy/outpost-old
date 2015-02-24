using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemySpawnActionHandler))]
public class SpawnActionsManagerEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
		EnemySpawnActionHandler myScript = (EnemySpawnActionHandler)target;

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Save Data"))
            {
                // saves file based on the level name that's loaded in scene
                string fileName = Application.streamingAssetsPath + "/" +  Application.loadedLevelName + ".xml";
                XMLParser<EnemySpawnAction>.XMLSerializer_List(myScript.container.SpawnAction_List, fileName);
            }
        }
    }
}