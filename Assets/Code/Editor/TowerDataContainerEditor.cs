using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Inspector button(s) to save/load/create tower data from XML.
/// Owner: John Fitzgerald
/// </summary>
[ExecuteInEditMode]
[CustomEditor(typeof(TowerDataContainer))]
public class TowerDataContainerEditor : Editor
{
    public bool ShowDebugLogs = true;

    private TowerDataContainer MyScript;
    private string XMLPath;

    private void OnEnable()
    {
        XMLPath = Application.streamingAssetsPath + "/TowerData.xml";
        MyScript = (TowerDataContainer)target;

        if (!IsListLoaded())
        {
            MyScript.TowerDataList = LoadFromXML();
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (IsListLoaded())
        {
			GUILayout.Label("THIS DATA IS FOR LOCAL XML ONLY");
			GUILayout.Label("DATA CANNOT BE UPDATED AT RUNTIME");
            NewItem_Button();
            EditorGUILayout.Space();
            SaveToXML_Button();
        }
        EditorGUILayout.Space();

        LoadFromXML_Button();
		LoadFromXML_Server_Button();
    }

    private bool IsListLoaded()
    {
        if (MyScript != null)
        {
            if (MyScript.TowerDataList.Count > 0)
                return true;
            else
                return false;
        }
        else
            return false;
    }

    /// <summary>
    /// Load XML file to List<T>.
    /// </summary>
    /// <returns></returns>
    private List<TowerData> LoadFromXML()
    {
        if (File.Exists(XMLPath))
        {
            // Sort by DisplayName before loading
            return XMLParser<TowerData>.XMLDeserializer_Local(XMLPath).OrderBy(o => o.DisplayName).ToList();//ThenBy(o => o.PlayerCount).ToList();
        }
        else
        {
            LogError("Cannot find Tower Data XML file");
            return null;
        }
    }

    /// <summary>
    /// Creates a new item in the Inspector List.
    /// </summary>
    private void NewItem_Button()
    {
        if (GUILayout.Button("Create Tower"))
        {
            MyScript.TowerDataList.Add(new TowerData("blank"));
        }
    }

    /// <summary>
    /// Saves current Inspector List to XML.
    /// </summary>
    private void SaveToXML_Button()
    {
        if (GUILayout.Button("Save Data"))
        {
			if (EditorUtility.DisplayDialog("Warning!", "You can only save to the LOCAL XML file. Are you sure you want to SAVE to the LOCAL XML file?", "Yes", "No"))
                XMLParser<TowerData>.XMLSerializer_Local(MyScript.TowerDataList, XMLPath);
        }
    }

    /// <summary>
    /// Load from Tower Data XML.
    /// </summary>
    private void LoadFromXML_Button()
    {
        if (GUILayout.Button("Load Data"))
        {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to LOAD?", "Yes", "No"))
            {
                MyScript.TowerDataList.Clear();
                if (!Application.isPlaying)
                    OnEnable();
                else
                    MyScript.TowerDataList = LoadFromXML();
            }
        }
    }

	private void LoadFromXML_Server_Button()
	{
		if (GUILayout.Button("Load SERVER Data"))
		{
			if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to LOAD data from the SERVER?", "Yes", "No"))
			{
				if (Application.isPlaying)
					EditorUtility.DisplayDialog("Nope.", "Data cannot be loaded form the SERVER during runtime.", "OK");
				else
				{
					MyScript.TowerDataList.Clear();
					MyScript.TowerDataList = LoadFromXML_Server();
				}
			}
		}
	}
	
	private List<TowerData> LoadFromXML_Server()
	{
		WWW www = new WWW("http://www.diademstudios.com/outpostdata/TowerData.xml");
		string myXML;
		
		while(!www.isDone)
		{
			
		}
		
		myXML = www.text;
		
		// Sort by StartTime and PlayerCount before loading
		return XMLParser<TowerData>.XMLDeserializer_Server(myXML).OrderBy(o => o.DisplayName).ToList();
	}

    /// <summary>
    /// Saves current Inspector List to XML.
    /// </summary>
    private void SaveToXML()
    {
        if (GUILayout.Button("Save Data"))
        {
            // saves file based on the level name that's loaded in scene
            string fileName = Application.streamingAssetsPath + "/TowerData.xml";

            XMLParser<TowerData>.XMLSerializer_Local(MyScript.TowerDataList, fileName);
        }
    }

    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[TowerDataContainerEditor] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[TowerDataContainerEditor] " + message);
    }
    #endregion
}