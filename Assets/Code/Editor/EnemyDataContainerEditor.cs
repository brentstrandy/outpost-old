using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
            MyScript.EnemyDataList = LoadFromXML_Local();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (IsListLoaded())
        {
            GUILayout.Label("THIS DATA IS FOR LOCAL XML ONLY");
            GUILayout.Label("DATA CANNOT BE UPDATED AT RUNTIME");
            EditorGUILayout.Space();
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
    private List<EnemyData> LoadFromXML_Local()
    {
        if (File.Exists(XMLPath))
        {
            // Sort by DisplayName before loading
            return XMLParser<EnemyData>.XMLDeserializer_Local(XMLPath).OrderBy(o => o.EnemyID).ToList();
        }
        else
        {
            LogError("Cannot find Enemy Data XML file");
            return null;
        }
    }

    private List<EnemyData> LoadFromXML_Server()
    {
		WWW www = new WWW("http://www.diademstudios.com/outpostdata/EnemyData_GetData.php");
        string myXML;

        while (!www.isDone)
        {
        }

        myXML = www.text;

        // Sort by StartTime and PlayerCount before loading
        return XMLParser<EnemyData>.XMLDeserializer_Server(myXML).OrderBy(o => o.EnemyID).ToList();
    }

    /*private List<EnemyData> LoadFromXML_Server()
    {
        return XMLParser<EnemyData>.XMLDeserializer_Data(
    }*/

    /// <summary>
    /// Saves current Inspector List to XML.
    /// </summary>
    private void SaveToXML_Button()
    {
        if (GUILayout.Button("Save LOCAL Data"))
        {
            if (EditorUtility.DisplayDialog("Warning!", "You can only save to the LOCAL XML file. Are you sure you want to SAVE to the LOCAL XML file?", "Yes", "No"))
                XMLParser<EnemyData>.XMLSerializer_Local(MyScript.EnemyDataList, XMLPath);
        }
    }

    private void SaveToXML_Server_Button()
    {
        /*
        // Create a form object for sending high score data to the server
        var form = new WWWForm();

        // The score
        form.AddField( "data", MyScript.EnemyDataList );

        // Create a download object
        var download = new WWW( "www.diademstudios.com/EnemyData.xml", form );

        // Wait until the download is done
        yield download;

        if(download.error)
            LogError("Error submitting XML: " + download.error);
        else
            Log("XML data was saved to server");
        */
    }

    /// <summary>
    /// Load from Enemy Data XML.
    /// </summary>
    private void LoadFromXML_Button()
    {
        if (GUILayout.Button("Load LOCAL Data"))
        {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to LOAD LOCAL data?", "Yes", "No"))
            {
                MyScript.EnemyDataList.Clear();
                if (!Application.isPlaying)
                    OnEnable();
                else
                    MyScript.EnemyDataList = LoadFromXML_Local();
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
                    MyScript.EnemyDataList.Clear();
                    MyScript.EnemyDataList = LoadFromXML_Server();
                }
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

    #endregion MessageHandling
}