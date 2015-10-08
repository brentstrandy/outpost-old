using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Inspector button(s) to save/edit level's spawn data from XML.
/// ReorderableList is an undocumented Unity container in UnityEditorInternal.
/// http://va.lent.in/unity-make-your-lists-functional-with-reorderablelist/
/// Owner: John Fitzgerald
/// </summary>
[ExecuteInEditMode]
[CustomEditor(typeof(EnemySpawnDataContainer))]
public class EnemySpawnDataContainerEditor : Editor
{
    public bool ShowDebugLogs = true;

    private EnemySpawnDataContainer MyScript;
    private ReorderableList ReorderList;
    private string XMLPath;

    private void OnEnable()
    {
        XMLPath = Application.streamingAssetsPath + "/" + "Level1" + ".xml"; // FITZGERALD: can't use Application.loadedLevelName in the editor (find way to be dynamic)
        MyScript = (EnemySpawnDataContainer)target;

        if (Application.isPlaying)
        {
            ReorderList = new ReorderableList(serializedObject, serializedObject.FindProperty("EnemySpawnDataList"),
                                              true, true, true, true); // dragable, header, add button, remove button
        }
        // takes over for EnemySpawnDataHandler
        else
        {
            if (!IsListLoaded())
            {
                //foreach (EnemySpawnData enemy in LoadFromXML())
                //    MyScript.EnemySpawnDataList.Add(enemy);

                MyScript.EnemySpawnDataList = LoadFromXML_Local();

                SortListByStartTime();
            }

            // load List into ReorderList for inspector
            ReorderList = new ReorderableList(serializedObject, serializedObject.FindProperty("EnemySpawnDataList"),
                                              true, true, true, true); // dragable,  header, add button, remove button
        }

        DisplayList();
        HeaderTitle();
        InhibitLastElementDeletion();
        DeletionPrompt();
        //DefaultAddEnemy();
        DropDownList();
    }

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        serializedObject.Update();
        ReorderList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();

        GUILayout.Label("THIS DATA IS FOR LOCAL XML ONLY");
        GUILayout.Label("DATA CANNOT BE UPDATED AT RUNTIME");
        // "Save Data" button at bottom of Inspector window
        if (IsListLoaded())
            SaveToXML_Button();

        EditorGUILayout.Space();

        LoadFromXML_Button();
    }

    /// <summary>
    /// !Application.isPlaying: Checks if List or ReorderableList are loaded
    /// </summary>
    /// <returns></returns>
    private bool IsListLoaded()
    {
        if (MyScript != null)
        {
            if (MyScript.EnemySpawnDataList.Count > 0)
                return true;
            else
                return false;
        }
        else
            return false;
    }

    /// <summary>
    /// Displays ReorderableList in inspector
    /// </summary>
    public void DisplayList()
    {
        // lambda
        ReorderList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = ReorderList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 6;

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight),
                                                         element.FindPropertyRelative("StartTime"), GUIContent.none);

            EditorGUI.PropertyField(new Rect(rect.x + 60, rect.y, 120, EditorGUIUtility.singleLineHeight),
                                             element.FindPropertyRelative("EnemyName"), GUIContent.none);

            EditorGUI.PropertyField(new Rect(rect.x + 190, rect.y, 30, EditorGUIUtility.singleLineHeight),
                                             element.FindPropertyRelative("StartAngle"), GUIContent.none);

            EditorGUI.PropertyField(new Rect(rect.x + 230, rect.y, 30, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("PlayerCount"), GUIContent.none);
        };
    }

    /// <summary>
    /// Change header title of ReorderableList
    /// </summary>
    public void HeaderTitle()
    {
        ReorderList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Enemy Spawn Editor", EditorStyles.boldLabel); };
    }

    /// <summary>
    /// Inhibits deletion of the last remaining element from ReorderableList
    /// </summary>
    public void InhibitLastElementDeletion()
    {
        ReorderList.onCanRemoveCallback = (ReorderableList l) => { return l.count > 1; };
    }

    /// <summary>
    /// Window prompts user before deleting a ReorderableList element
    /// </summary>
    public void DeletionPrompt()
    {
        // deletion prompt
        ReorderList.onRemoveCallback = (ReorderableList l) =>
        {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete?", "Yes", "No"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            }
        };
    }

    /// <summary>
    /// Default Enemy to add to ReorderableList
    /// </summary>
    public void DefaultAddEnemy()
    {
        ReorderList.onAddCallback = (ReorderableList l) =>
        {
            var index = l.serializedProperty.arraySize;
            l.serializedProperty.arraySize++;
            l.index = index;
            var element = l.serializedProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("EnemyName").stringValue = "LightSpeeder";
            element.FindPropertyRelative("StartTime").floatValue = 0;
            element.FindPropertyRelative("StartAngle").floatValue = 0;
            element.FindPropertyRelative("PlayerCount").floatValue = 1;
        };
    }

    /// <summary>
    /// Drop down menu for ReorderableList enemy selection
    /// </summary>
    public void DropDownList()
    {
        ReorderList.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>
        {
            GenericMenu menu = new GenericMenu();

            // list of available enemies to add (returns string as gibberish -- asset addresses?)
            string[] availableEnemies = AssetDatabase.FindAssets("", new[] { "Assets/Resources/Enemies" });
            string[] enemyNames = Directory.GetFiles(Application.dataPath + "/Resources/Enemies").Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
            enemyNames = enemyNames.Where(name => !name.Contains(".prefab")).ToArray();

            foreach (string enemy in availableEnemies)
            {
                string path = AssetDatabase.GUIDToAssetPath(enemy);
                // add enemy to menu choices
                menu.AddItem(new GUIContent("Enemy Type/" + Path.GetFileNameWithoutExtension(path)), false, ClickHandler,
                             new EnemyCreationParams() { AssetPath = path, EnemyName = path, HighlightColor = new Color(10, 10, 10) });
            }

            #region for other menus (Boss enemy vs Normal enemy)

            //guids = AssetDatabase.FindAssets("", new[] { "Assets/Prefabs/Bosses" });
            //foreach (var guid in guids)
            //{
            //    var path = AssetDatabase.GUIDToAssetPath(guid);
            //    menu.AddItem(new GUIContent("Bosses/" + Path.GetFileNameWithoutExtension(path)),
            //    false, ClickHandler,
            //    new WaveCreationParams() { Type = EnemySpawnData.EnemyType.Boss, Path = path });
            //}

            #endregion for other menus (Boss enemy vs Normal enemy)

            menu.ShowAsContext();
        };
    }

    /// <summary>
    /// Click handler for ReorderableList's add button
    /// </summary>
    /// <param name="target"></param>
    private void ClickHandler(object target)
    {
        var data = (EnemyCreationParams)target;
        var index = ReorderList.serializedProperty.arraySize;

        // Increase ReorderableList array size for the new Enemy
        ReorderList.serializedProperty.arraySize++;
        ReorderList.index = index;

        // The new Enemy added by user
        var element = ReorderList.serializedProperty.GetArrayElementAtIndex(index);

        // EnemyName
        element.FindPropertyRelative("EnemyName").stringValue = data.EnemyName;

        element.FindPropertyRelative("PrefabName").stringValue = data.PrefabName;

        // StartTime
        element.FindPropertyRelative("StartTime").floatValue = 60f;

        // StartAngle (increment from last angle in ReorderableList?)
        element.FindPropertyRelative("StartAngle").intValue = 360;

        // Color
        element.FindPropertyRelative("HighlightColor").colorValue = new Color(10, 10, 10);

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Scruct for ReorderableList functionality
    /// </summary>
    private struct EnemyCreationParams
    {
        public string AssetPath; // if we want to load from Assets folder
        private string _EnemyName;

        public string EnemyName
        {
            get { return _EnemyName; }
            set
            {
                if (value.GetType() == typeof(string))
                    _EnemyName = Path.GetFileNameWithoutExtension(value);
                else
                    _EnemyName = "*unkown*";
            }
        }

        public string PrefabName;
        public Color HighlightColor;
    }

    /// <summary>
    /// !Application.isPlaying: sorts list by start time
    /// </summary>
    private void SortListByStartTime()
    {
        if (MyScript.EnemySpawnDataList != null)
        {
            MyScript.EnemySpawnDataList = MyScript.EnemySpawnDataList.OrderBy(o => o.StartTime).ToList();
        }
    }

    /// <summary>
    /// Load XML file to List<T>
    /// </summary>
    /// <returns></returns>
    private List<EnemySpawnData> LoadFromXML_Local()
    {
        // Sort by StartTime and PlayerCount before loading
        return XMLParser<EnemySpawnData>.XMLDeserializer_Local(XMLPath).OrderBy(o => o.StartTime).ThenBy(o => o.PlayerCount).ToList();
    }

    /// <summary>
    /// Saves current Inspector List to XML
    /// </summary>
    private void SaveToXML_Button()
    {
        if (GUILayout.Button("Save Data"))
        {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to SAVE?", "Yes", "No"))
                XMLParser<EnemySpawnData>.XMLSerializer_Local(MyScript.EnemySpawnDataList, XMLPath);
        }
    }

    /// <summary>
    /// Saves current Inspector List to XML
    /// </summary>
    private void LoadFromXML_Button()
    {
        if (GUILayout.Button("Load LOCAL Data"))
        {
            if (EditorUtility.DisplayDialog("Warning!", "Data will be loaded from the LOCAL XML file. Are you sure you want to LOAD?", "Yes", "No"))
            {
                MyScript.EnemySpawnDataList.Clear();

                if (!Application.isPlaying)
                    OnEnable();
                else
                    MyScript.EnemySpawnDataList = LoadFromXML_Local();
            }
        }
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[EnemySpawnDataContainerEditor] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[EnemySpawnDataContainerEditor] " + message);
    }

    #endregion MessageHandling
}