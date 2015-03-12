using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector button(s) to save/load level's tower data from XML.
/// Owner: John Fitzgerald
/// </summary>
[CustomEditor(typeof(TowerDataContainer))]
public class TowerDataContainerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            TowerDataContainer myScript = (TowerDataContainer)target;

            if (GUILayout.Button("Save Data"))
            {
                // saves file based on the level name that's loaded in scene
                string fileName = Application.streamingAssetsPath + "/TowerData.xml";

                XMLParser<TowerData>.XMLSerializer_List(myScript.TowerDataList, fileName);
            }
        }
    }
}