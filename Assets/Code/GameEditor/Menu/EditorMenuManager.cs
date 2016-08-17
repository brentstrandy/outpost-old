using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages high-level menu operations. Is responsible for transitioning to new menus
/// Owner: Brent Strandy
/// </summary>
public class EditorMenuManager : MonoBehaviour
{
    private static EditorMenuManager instance;

    public bool ShowDebugLogs = true;

    private LevelData CurrentLevelData;

    // Menu Panels
    public GameObject StartMenuPanel;
    public GameObject MainMenuPanel;
    public GameObject LevelEditorPanel;

    public GameObject CurrentMenuPanel { get; private set; }

    #region INSTANCE (SINGLETON)

    /// <summary>
    /// Singleton - There can be only one
    /// </summary>
    /// <value>The instance.</value>
    public static EditorMenuManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<EditorMenuManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    #endregion INSTANCE (SINGLETON)

    private void Start()
    {
        // Set the current menu panel as the start menu
        SetCurrentMenuPanel(StartMenuPanel);
    }

    #region MENU TRANSITIONS

    public void ShowStartMenuPanel()
    {
        SetCurrentMenuPanel(StartMenuPanel);
    }

    public void ShowMainMenuPanel()
    {
        SetCurrentMenuPanel(MainMenuPanel);
    }

    public void ShowLevelEditorPanel()
    {
    	SetCurrentMenuPanel(LevelEditorPanel);
    }

    #endregion MENU TRANSITIONS

    /// <summary>
    /// Set the currently visible menu panel
    /// </summary>
    /// <param name="newMenuPanel">New menu panel</param>
    private void SetCurrentMenuPanel(GameObject newMenuPanel)
    {
        // Deactivate the current panel
        if (CurrentMenuPanel != null)
            CurrentMenuPanel.SetActive(false);

        // Activate the new panel
        if (newMenuPanel != null)
            newMenuPanel.SetActive(true);

        CurrentMenuPanel = newMenuPanel;
    }

    public LevelData LoadLevel(string levelTitle)
    {
    	// TODO: Update with new GameDataManager functionality
    	CurrentLevelData = GameDataManager.Instance.FindLevelDataByDisplayName(levelTitle);

    	return CurrentLevelData;
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[EditorMenuManager] " + message);
    }

    protected void LogError(string message)
    {
        Debug.LogError("[EditorMenuManager] " + message);
    }

    #endregion MessageHandling
}