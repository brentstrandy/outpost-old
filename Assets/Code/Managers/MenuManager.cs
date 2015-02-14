using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour 
{
	private static MenuManager instance;

	public bool ShowDebugLogs = true;
	public Camera GUICamera;
	public Camera GameCamera;

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
		// Set the current menu panel as the start menu
		SetCurrentMenuPanel(StartMenuPanel);

        // Track events in order to react to Session Manager events as they happen
		SessionManager.Instance.OnSMConnectionFail += DisconnectedFromNetwork;
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
	public void ShowStartGame()
	{
		// Set the current menu to null - clearing any menu
		SetCurrentMenuPanel(null);

		// For now, simply just switch cameras
		GUICamera.enabled = false;
		GameCamera.enabled = true;

		GameManager.Instance.StartNewGame();
	}

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

	public void ShowRoomDetailsMenu(bool host = false)
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
		if(newMenuPanel != null)
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
