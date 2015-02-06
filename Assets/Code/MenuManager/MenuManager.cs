using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour 
{
	private static MenuManager instance;

	public bool ShowDebugLogs = true;

	// Menu Panels
	public GameObject StartMenuPanel;
	public GameObject MainMenuPanel;
	public GameObject RoomDetailsPanel;
	public GameObject JoinGamePanel;
	public GameObject MatchmakingGamePanel;
	public GameObject MainSettingsPanel;
	public GameObject AudioSettingsPanel;
	public GameObject VideoSettingsPanel;
	public GameObject ControlSettingsPanel;
	public GameObject GameplaySettingsPanel;

	private GameObject CurrentMenuPanel;

    #region Lists of Legacy GUI Menu variables
    //public List<GameObject> Canvases, Panels, MenuHeaders, Buttons;
    //[HideInInspector]
    //public string CurrentMenu, PreviousMenu;
    //[HideInInspector]
    //public GameObject CurrentMenu_GO, PreviousMenu_GO;
    #endregion

    public AudioClip ShortClick, LongClick;

	/// <summary>
	/// Persistent Singleton - this object lasts between scenes
	/// </summary>
	/// <value>The instance.</value>
	public static MenuManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<MenuManager>();
				
				//Tell unity not to destroy this object when loading a new scene!
				DontDestroyOnLoad(instance.gameObject);
			}
			
			return instance;
		}
	}

	void Start ()
	{
        #region Audio
        string soundEffectsPath = "Audio/Effects/";
        ShortClick = Resources.Load(soundEffectsPath + "short_click_01") as AudioClip;
        LongClick = Resources.Load(soundEffectsPath + "long_click_01") as AudioClip;
        #endregion

		// Set the current menu panel as the start menu
		SetCurrentMenuPanel(StartMenuPanel);

        #region Network Session
        // Track events in order to react to Session Manager events as they happen
		SessionManager.Instance.OnSMConnectionFail += DisconnectedFromNetwork;
        #endregion
    }

    void Update ()
    {

    }

    public void PlayShortClick()
    {
        audio.PlayOneShot(ShortClick);
    }

    public void PlayLongClick()
    {
        audio.PlayOneShot(LongClick);
    }


	#region Menu Transitions
	public void ShowJoinGameMenu()
	{
		SetCurrentMenuPanel(JoinGamePanel);
	}

	public void ShowMatchmakingGameMenu()
	{
		SetCurrentMenuPanel(MatchmakingGamePanel);
	}

	public void ShowMainMenu()
	{
		SetCurrentMenuPanel(MainMenuPanel);
	}

	public void ShowRoomDetailsMenu()
	{
		SetCurrentMenuPanel(RoomDetailsPanel);
	}

	public void ShowSettingsMenu()
	{
		SetCurrentMenuPanel(MainSettingsPanel);
	}

	public void ShowAudioSettingsMenu()
	{
		SetCurrentMenuPanel(AudioSettingsPanel);
	}

	public void ShowVideoSettingsMenu()
	{
		SetCurrentMenuPanel(VideoSettingsPanel);
	}

	public void ShowControlSettingsMenu()
	{
		SetCurrentMenuPanel(ControlSettingsPanel);
	}

	public void ShowGameplaySettingsMenu()
	{
		SetCurrentMenuPanel(GameplaySettingsPanel);
	}
	#endregion

	/// <summary>
	/// Called whenever the SessionManager joins a lobby
	/// </summary>
	/*private void JoinedLobby()
	{
		int index = 0;

		SetCurrentMenuPanel(JoinGamePanel);

		// Get the list of rooms currently in the lobby
		RoomList = SessionManager.Instance.GetRoomList();
		Debug.Log("Room Count: " + RoomList.Length);
		// Display each room currently in the lobby
		foreach(Room room in RoomList)
		{
			// Instantiate row for each room and add it as a child of the JoinGame UI Panel
			GameObject obj = Instantiate(JoinGameDetails_Prefab, new Vector3(0, -70 + (-70 * index), 0), Quaternion.identity) as GameObject;
			obj.GetComponentInChildren<Text>().text = room.name;
			//obj.GetComponent<Button>().onClick.AddListener(() => JoinRoom_Click());
			obj.transform.parent = JoinGamePanel.transform;
			index++;
		}
	}*/

	/// <summary>
	/// Called whenever the SessionManager is disconnected from the network
	/// </summary>
	/// <param name="cause">Cause.</param>
	private void DisconnectedFromNetwork(DisconnectCause cause)
	{
		SetCurrentMenuPanel(StartMenuPanel);
	}

	/// <summary>
	/// Set the currently visible menu panel
	/// </summary>
	/// <param name="newMenuPanel">New menu panel</param>
	private void SetCurrentMenuPanel(GameObject newMenuPanel)
	{
		// Deactivate the current panel
		if(CurrentMenuPanel != null)
			CurrentMenuPanel.SetActive(false);
		// Activate the new panel
		newMenuPanel.SetActive(true);
		CurrentMenuPanel = newMenuPanel;
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[MenuManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[MenuManager] " + message);
	}
	#endregion
}
