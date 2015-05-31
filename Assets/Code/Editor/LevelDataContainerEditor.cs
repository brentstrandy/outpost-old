using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
            NewItem_Button();
            EditorGUILayout.Space();
            SaveToXML_Button();
        }

        EditorGUILayout.Space();

        LoadFromXML_Button();
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
            return XMLParser<LevelData>.XMLDeserializer_Local(XMLPath).OrderBy(o => o.DisplayName).ToList();
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
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to SAVE?", "Yes", "No"))
                XMLParser<LevelData>.XMLSerializer_Local(MyScript.LevelDataList, XMLPath);
        }
    }

    /// <summary>
    /// Load from Level Data XML.
    /// </summary>
    private void LoadFromXML_Button()
    {
        if (GUILayout.Button("Load Data"))
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
    #endregion
}