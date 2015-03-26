using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;
using System.Linq;

/// <summary>
/// Inspector button(s) to save/edit level's spawn data from XML.
/// ReorderableList is an undocumented Unity container in UnityEditorInternal.
/// http://va.lent.in/unity-make-your-lists-functional-with-reorderablelist/
/// Owner: John Fitzgerald
/// </summary>
[CustomEditor(typeof(EnemySpawnDataContainer))]
public class EnemySpawnDataContainerEditor : Editor 
{
    private EnemySpawnDataContainer MyScript;
    private ReorderableList ReorderList;

    private void OnEnable ()
    {
        MyScript = (EnemySpawnDataContainer)target;
        ReorderList = new ReorderableList(serializedObject, serializedObject.FindProperty("EnemySpawnDataList"), 
                                          true, true, true, true); // dragable, header, add button, remove button

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

        // "Save Data" button at bottom of Inspector window
        if (Application.isPlaying)
            SaveToXML();
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
                             new EnemyCreationParams() { Type = EnemySpawnData.EnemyType.Normal, AssetPath = path, EnemyName = path });
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
            #endregion

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

        // StartTime (add default value greater than last Enemy in ReordableList if EnemyType == Boss)
        element.FindPropertyRelative("StartTime").floatValue =
            data.Type == EnemySpawnData.EnemyType.Normal ? 60f : 60f;
       
        // StartAngle (increment from last angle in ReorderableList?)
        element.FindPropertyRelative("StartAngle").intValue = 360;

        // EnemyType
        element.FindPropertyRelative("Type").enumValueIndex = (int)data.Type;
        
        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Scruct for ReorderableList functionality
    /// </summary>
    private struct EnemyCreationParams
    {
        public EnemySpawnData.EnemyType Type;
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
    }

    /// <summary>
    /// Saves current Inspector List to XML
    /// </summary>
    private void SaveToXML()
    {
        if (GUILayout.Button("Save Data"))
        {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to save?", "Yes", "No"))
            {
                // saves file based on the level name that's loaded in scene
                string fileName = Application.streamingAssetsPath + "/" + Application.loadedLevelName + ".xml";

                XMLParser<EnemySpawnData>.XMLSerializer_List(MyScript.EnemySpawnDataList, fileName);
            }
        }
    }
}