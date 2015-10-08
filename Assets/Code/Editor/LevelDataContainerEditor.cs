using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector button(s) to save/load/create level data from XML.
/// Owner: John Fitzgerald
/// </summary>
[ExecuteInEditMode]
[CustomEditor(typeof(LevelDataContainer))]
public class LevelDataContainerEditor : Editor
{
    public bool ShowDebugLogs = true;

    private LevelDataContainer MyScript;
    private string XMLPath;

    private void OnEnable()
    {
        XMLPath = Application.streamingAssetsPath + "/LevelData.xml";
        MyScript = (LevelDataContainer)target;

        if (!IsListLoaded())
        {
            MyScript.LevelDataList = LoadFromXML();
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
            if (MyScript.LevelDataList.Count > 0)
                return true;
            else
                return false;
        }
        else
            return false;
    }

    /// <summary>
    /// Creates a new item in the Inspector List.
    /// </summary>
    private void NewItem_Button()
    {
        if (GUILayout.Button("Create Level"))
        {
            MyScript.LevelDataList.Add(new LevelData("blank"));
        }
    }

    /// <summary>
    /// Load XML file to List<T>.
    /// </summary>
    /// <returns></returns>
    private List<LevelData> LoadFromXML()
    {
        if (File.Exists(XMLPath))
        {
            // Sort by DisplayName before loading
            return XMLParser<LevelData>.XMLDeserializer_Local(XMLPath).OrderBy(o => o.LevelID).ToList();
        }
        else
        {
            LogError("Cannot find Level Data XML file");
            return null;
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
                XMLParser<LevelData>.XMLSerializer_Local(MyScript.LevelDataList, XMLPath);
        }
    }

    /// <summary>
    /// Load from Level Data XML.
    /// </summary>
    private void LoadFromXML_Button()
    {
        if (GUILayout.Button("Load LOCAL Data"))
        {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to LOAD?", "Yes", "No"))
            {
                MyScript.LevelDataList.Clear();
                if (!Application.isPlaying)
                    OnEnable();
                else
                    MyScript.LevelDataList = LoadFromXML();
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
                    MyScript.LevelDataList.Clear();
                    MyScript.LevelDataList = LoadFromXML_Server();
                }
            }
        }
    }

    private List<LevelData> LoadFromXML_Server()
    {
        WWW www = new WWW("http://www.diademstudios.com/outpostdata/LevelData.xml");
        string myXML;

        while (!www.isDone)
        {
        }

        myXML = www.text;

        // Sort by StartTime and PlayerCount before loading
        return XMLParser<LevelData>.XMLDeserializer_Server(myXML).OrderBy(o => o.DisplayName).ToList();
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[LevelDataContainerEditor] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[LevelDataContainerEditor] " + message);
    }

    #endregion MessageHandling
}