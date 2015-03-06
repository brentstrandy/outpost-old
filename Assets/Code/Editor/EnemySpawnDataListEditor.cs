using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemySpawnEditor))]
public class EnemySpawnDataListEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		if(Application.isEditor)
		{
			if (target.GetType() == typeof(EnemySpawnEditor))
			{
	            if (GUILayout.Button("Save Data"))
	            {
	                // saves file based on the level name that's loaded in scene
	                string fileName = Application.streamingAssetsPath + "/" +  Application.loadedLevelName + ".xml";
					XMLParser<EnemySpawnData>.XMLSerializer_List(((EnemySpawnEditor)target).SpawnActionList, fileName);
	            }
	        }
		}
    }
}