using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

/// <summary>
/// Inspector button(s) to save/load level's tower data from XML.
/// Owner: John Fitzgerald
/// </summary>
[CustomEditor(typeof(TowerDataContainer))]
public class TowerDataContainerEditor : Editor
{
    private TowerDataContainer MyScript;

    private void OnEnable()
    {
        MyScript = (TowerDataContainer)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (Application.isPlaying)
            SaveToXML();
    }

    /// <summary>
    /// Saves current Inspector List to XML
    /// </summary>
    private void SaveToXML()
    {
        if (GUILayout.Button("Save Data"))
        {
            // saves file based on the level name that's loaded in scene
            string fileName = Application.streamingAssetsPath + "/TowerData.xml";

            XMLParser<TowerData>.XMLSerializer_List(MyScript.TowerDataList, fileName);
        }
    }
}