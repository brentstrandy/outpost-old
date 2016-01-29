using UnityEngine;

/// <summary>
/// Allows debug information to be displayed on screen for testing purposes.
/// Created By: John Fitzgerald
/// </summary>
public class DebugButtonManager : MonoBehaviour 
{
    private static DebugButtonManager instance;
    public bool ShowDebugLogs = true;


    #region INSTANCE (SINGLETON)

    /// <summary>
    /// Singleton - There can only be one
    /// </summary>
    /// <value>The instance.</value>
    public static DebugButtonManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<DebugButtonManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    #endregion INSTANCE (SINGLETON)
	
	// Update is called once per frame
	void Update () 
    {
        // If a level is running, name is returned from GameManager
        if (GameManager.Instance != null)
        {
            // Only check for input if the game is currently running
            if (GameManager.Instance.GameRunning)
            {
                //if (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Question))
                if (Input.GetKeyDown(KeyCode.Slash) && Input.GetKey("left shift") || Input.GetKeyDown("left shift") && Input.GetKey(KeyCode.Slash))
				{
					string copyText = "";
					copyText = "Date/Time: " + System.DateTime.Now.ToString() + "\n";
					copyText += "Player Name: " + PlayerManager.Instance.CurPlayer.Username + " (" + PlayerManager.Instance.CurPlayer.AccountID + ")\n";
					copyText += "Level Name: " + GameManager.Instance.CurrentLevelData.DisplayName + "\n";
					copyText += "Player Count: " + SessionManager.Instance.GetRoomPlayerCount().ToString() + "\n";
					copyText += "Game Host: " + SessionManager.Instance.GetPlayerInfo().isMasterClient.ToString() + "\n";
					copyText += "Level Time: " + Mathf.RoundToInt(Time.time - GameManager.Instance.LevelStartTime).ToString();

					TextEditor te = new TextEditor();
					te.text = new GUIContent(copyText).ToString();
					te.SelectAll();
					te.Copy();

					NotificationManager.Instance.DisplayNotification(new NotificationData("Debug Info", "Copied Information to clipboard", "QuickInfo"));
                }
            }
        }
        // If pregame, name is returned from MenuManager
        else
            //if (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Question))
            if (Input.GetKeyDown(KeyCode.Slash) && Input.GetKey("left shift") || Input.GetKeyDown("left shift") && Input.GetKey(KeyCode.Slash))
            {
                // Display level name
                LogError("Name of Location: " + MenuManager.Instance.CurrentMenuPanel.name);
            }
	}

    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[DebugButtonManager] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[DebugButtonManager] " + message);
    }
    #endregion MessageHandling
}
