using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector button(s) to save/load level's spawn data from XML.
/// Owner: John Fitzgerald
/// </summary>
[CustomEditor(typeof(EnemySpawnDataContainer))]
public class EnemySpawnDataContainerEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		if(Application.isPlaying)
		{
            EnemySpawnDataContainer myScript = (EnemySpawnDataContainer)target;
                
	        if (GUILayout.Button("Save Data"))
	        {
	            // saves file based on the level name that's loaded in scene
	            string fileName = Application.streamingAssetsPath + "/" +  Application.loadedLevelName + ".xml";

				XMLParser<EnemySpawnData>.XMLSerializer_List(myScript.SpawnDataList, fileName);
	        }
		}
    }
}