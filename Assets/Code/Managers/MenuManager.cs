using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages high-level menu operations. Is responsible for transitioning to new menus
/// Owner: John Fitzgerald
/// </summary>
public class MenuManager : MonoBehaviour
{
	private static MenuManager instance;
	
	public bool ShowDebugLogs = true;
	// Need a handle to the GUI camera in order to transition back to the camera
	public Camera GUICamera;
	public Camera GameCamera;
	
	public LevelData CurrentLevelData { get; private set; }
	
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
	public GameObject InGamePanel;
	public GameObject LoadLevelPanel;
	public GameObject EndGamePanel;
	
	private GameObject CurrentMenuPanel;
	
	#region INSTANCE (SINGLETON)
	/// <summary>
	/// Singleton - There can be only one
	/// </summary>
	/// <value>The instance.</value>
	public static MenuManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<MenuManager>();
			}
			
			return instance;
		}
	}
	
	void Awake()
	{
		instance = this;
	}
	#endregion
	
	void Start ()
	{
		// No level is being played when the game starts
		CurrentLevelData = null;
		
		// Set the current menu panel as the start menu
		SetCurrentMenuPanel(StartMenuPanel);
		
		// Track events in order to react to Session Manager events as they happen
		SessionManager.Instance.OnSMConnectionFail += DisconnectedFromNetwork;
	}
	
	#region MENU TRANSITIONS
	public void ShowStartGame(LevelData levelData)
	{
		// TO DO: Lock the room - no one else can join
		
		// Save a reference to the current level's data
		CurrentLevelData = levelData;
		
		// Show the loading screen as the level is loaded
		LoadLevelPanel.SetActive(true);
		
		// Show the InGame menu (behind the loading menu)
		SetCurrentMenuPanel(InGamePanel);
		
		// Load the selected level
		Application.LoadLevelAsync(CurrentLevelData.SceneName);
	}
	
	public void ReturnToRoomDetailsMenu()
	{
		// Return to the main game scene where the main menu functionality takes place
		Application.LoadLevel("MainGame");
		
		// Show the Room Details menu
		SetCurrentMenuPanel(RoomDetailsPanel);
		
		// No level is being played when in the menu
		CurrentLevelData = null;
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
	
	public void ShowVictoryMenu()
	{
		SetCurrentMenuPanel(EndGamePanel);
	}
	
	public void ShowLossMenu()
	{
		SetCurrentMenuPanel(EndGamePanel);
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