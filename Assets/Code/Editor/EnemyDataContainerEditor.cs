using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Inspector button(s) to save/load/create enemy data from XML.
/// Owner: John Fitzgerald
/// </summary>
[ExecuteInEditMode]
[CustomEditor(typeof(EnemyDataContainer))]
public class EnemyDataContainerEditor : Editor
{
    public bool ShowDebugLogs = true;

    private EnemyDataContainer MyScript;
    private string XMLPath;

    private void OnEnable()
    {
        XMLPath = Application.streamingAssetsPath + "/EnemyData.xml";
        MyScript = (EnemyDataContainer)target;

        if (!IsListLoaded())
            MyScript.EnemyDataList = LoadFromXML();
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
            if (MyScript.EnemyDataList.Count > 0)
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
        if (GUILayout.Button("Create Enemy"))
        {
            MyScript.EnemyDataList.Add(new EnemyData("blank"));
        }
    }

    /// <summary>
    /// Load XML file to List<T>.
    /// </summary>
    /// <returns></returns>
    private List<EnemyData> LoadFromXML()
    {
        if (File.Exists(XMLPath))
        {
            // Sort by DisplayName before loading
            return XMLParser<EnemyData>.XMLDeserializer_List(XMLPath).OrderBy(o => o.DisplayName).ToList();//ThenBy(o => o.PlayerCount).ToList();
        }
        else
        {
            LogError("Cannot find Enemy Data XML file");
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
                XMLParser<EnemyData>.XMLSerializer_List(MyScript.EnemyDataList, XMLPath);
        }
    }

    /// <summary>
    /// Load from Enemy Data XML.
    /// </summary>
    private void LoadFromXML_Button()
    {
        if (GUILayout.Button("Load Data"))
        {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to LOAD?", "Yes", "No"))
            {
                MyScript.EnemyDataList.Clear();
                if (!Application.isPlaying)
                    OnEnable();
                else
                    MyScript.EnemyDataList = LoadFromXML();
            }
        }
    }

    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[EnemyDataContainerEditor] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[EnemyDataContainerEditor] " + message);
    }
    #endregion
}