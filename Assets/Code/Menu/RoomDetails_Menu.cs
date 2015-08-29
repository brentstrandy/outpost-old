using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RoomDetails_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	// Handles to GUI objects
	public GameObject[] PlayerName_GUIText;
	public GameObject RoomTitle_GUIText;
	public GameObject Chat_GUIText;
	public GameObject SendChat_GUIInput;
	public GameObject StartGame_GUIButton;
	public GameObject LevelTitle_GUILabel;

	/// <summary>
	/// Tracks GUI objects for each tower that is selected for the Loadout
	/// </summary>
	private List<GameObject> SelectedTowerButtonList;
	private List<GameObject> TowerButtonList;
    private GameObject LevelLoadoutSelection;
    /// <summary>
	/// Tracks TowerData for each tower that is selected for the loadout
	/// </summary>
	private List<TowerData> TowerLoadoutData;
    /// <summary>
	/// Tracks LevelData for the level that is selected for the Loadout
    /// </summary>
    private LevelData LevelLoadoutData;
    private bool LevelSelected;
	private string AvailableColorIndexes = "01234567";

	private PhotonView ObjPhotonView;

	public void Awake()
	{
		TowerButtonList = new List<GameObject>();
		SelectedTowerButtonList = new List<GameObject>();
		TowerLoadoutData = new List<TowerData>();

		// Save a handle to the photon view associated with this GameObject for use later
		ObjPhotonView = PhotonView.Get(this);
	}

	/// <summary>
	/// OnEnable is called when the menu is triggered to be displayed
	/// </summary>
	private void OnEnable()
	{
		// Establish listeners for all applicable events (these listeners are removed when the menu is hidden
		// so that they are not triggered erroneously)
		SessionManager.Instance.OnSMPlayerJoinedRoom += PlayerJoinedRoom_Event;
		SessionManager.Instance.OnSMPlayerLeftRoom += PlayerLeftRoom_Event;
		SessionManager.Instance.OnSMLeftRoom += KickedFromRoom_Event;
		SessionManager.Instance.OnSMSwitchMaster += MasterClientSwitched_Event;
		
		Initialize();
	}

	/// <summary>
	/// OnDisaple is called when the menu is triggered to be hidden
	/// </summary>
	private void OnDisable()
	{
		// Remove listeners for all applicable events (these listeners are added again when the menu is shown)
		SessionManager.Instance.OnSMPlayerJoinedRoom -= PlayerJoinedRoom_Event;
		SessionManager.Instance.OnSMPlayerLeftRoom -= PlayerLeftRoom_Event;
		SessionManager.Instance.OnSMLeftRoom -= KickedFromRoom_Event;

		// Clear chat history so the next room will start with a clear chat area
		Chat_GUIText.GetComponent<Text>().text = "";
	}

	private void Initialize()
	{
		// See if the player is currently the room owner or just joining
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			StartGame_GUIButton.SetActive(true);
			SessionManager.Instance.SetRoomVisibility(true);
		}
		else
			StartGame_GUIButton.SetActive(false);

		// Immediately refresh the player name list to show the host
		RefreshPlayerNames();
		
		// Immediately refresh the room details
		RefreshRoomDetails();
		
		// Add Level Buttons for each Level (this needs to be done before the tower buttons because the towers depend on the LevelData)
		InitiateLevelButtons();
		
		// The towers are automatically refreshed when the level buttons are initiated
		//RefreshTowerButtons();
	}
	
	#region ON CLICK
	/// <summary>
	/// Used by the GUI system to start the game when the Start button is Clicked
	/// </summary>
	public void StartGame_Click()
	{
		// only starts game if the user has selected a level
		if (LevelSelected)
		{
			// Hide the room from other players (but still keep it open to be joined by invite)
			if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
				SessionManager.Instance.SetRoomVisibility(false);
			// Tell all the clients to load the level
			ObjPhotonView.RPC ("LoadLevel", PhotonTargets.All, null);
		}
	}

	/// <summary>
	/// Used by the GUI system to go leave the room when the Back button is pressed
	/// </summary>
	public void Back_Click()
	{
		// TO DO: Ask the user if they're sure they want to leave

		SessionManager.Instance.LeaveRoom();

		// Tell the MenuManager to transition back
		MenuManager.Instance.ShowMainMenu();
	}

	/// <summary>
	/// Used by the GUI system to send a chat message to all clients when the Send button is pressed
	/// </summary>
	public void SendChat_Click()
	{
		// Only send something if there is something to send
		if(SendChat_GUIInput.GetComponent<InputField>().text != "")
		{
			// Tell all clients that a chat message has been sent
			ObjPhotonView.RPC("ReceiveChatMessage", PhotonTargets.All, SessionManager.Instance.GetPlayerInfo().name, SendChat_GUIInput.GetComponent<InputField>().text);
			// Clear chat message from the GUI Input field to show it was sent
			SendChat_GUIInput.GetComponent<InputField>().text = "";
		}
	}

	/// <summary>
	/// Click event called when the player selects a tower within this menu
	/// </summary>
	/// <param name="towerButton">Tower button.</param>
	/// <param name="towerData">Tower data.</param>
	public void TowerButton_Click(GameObject towerButton, TowerData towerData)
	{
		if(towerButton.GetComponent<TowerButton>().Selected)
		{
			SelectedTowerButtonList.Remove(towerButton);
			TowerLoadoutData.Remove(towerData);

			// Tell the button to unselect itself
			towerButton.GetComponent<TowerButton>().UnselectTower();
		}
		else
		{
			// Add the new tower to the loadout
			SelectedTowerButtonList.Add(towerButton);
			TowerLoadoutData.Add(towerData);

			// Tell the button to select itself
			towerButton.GetComponent<TowerButton>().SelectTower();
		}
	}

    /// <summary>
    /// Click event called when the player selects a level within this menu
    /// </summary>	
    //public void LevelButton_Click(GameObject levelButton, LevelData levelData)
	private void LevelButton_Click(GameObject levelButton, LevelData levelData)
    {
		// Unselect the previous level (if there was a previously selected level)
		if(LevelLoadoutSelection != null)
			LevelLoadoutSelection.GetComponent<LevelButton>().UnselectLevel();

		// Updates currently selected level
		LevelLoadoutSelection = levelButton;

		// Tell the button to select itself
		LevelLoadoutSelection.GetComponent<LevelButton>().SelectLevel();

		// Tell all other clients that a new level has been selected
		ObjPhotonView.RPC("NewLevelSelected", PhotonTargets.Others, levelData.DisplayName);

		// Sets level to be loaded
		LevelLoadoutData = levelData;
		LevelSelected = true;

		// Update the available Towers to choose from
		RefreshTowerButtons();
		
		// Update whether or not the start button is enabled based on the current number of players in the room
		RefreshStartGameButton();
    }

	public void KickPlayerButton_Click(PhotonPlayer player)
	{
		SessionManager.Instance.KickPlayer(player);
	}
	#endregion
	
	#region EVENTS
	/// <summary>
	/// Event Listener that is triggered when a player joins the room
	/// </summary>
	/// <param name="player">Player Data</param>
	private void PlayerJoinedRoom_Event(PhotonPlayer player)
	{
		// Give the player a color (assign it only if player is the Master Client)
		int colorIndex = AssignColor();
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "PlayerColorIndex", colorIndex } });
		
		// Tell the new Player which level is currently selected
		ObjPhotonView.RPC("NewLevelSelected", player, LevelLoadoutData.DisplayName);
		
		RefreshPlayerNames();
		
		// Update whether or not the start button is enabled based on the current number of players in the room
		RefreshStartGameButton();
	}

	/// <summary>
	/// Event Listener that is triggered when a player leaves the room
	/// </summary>
	/// <param name="player">Player.</param>
	private void PlayerLeftRoom_Event(PhotonPlayer player)
	{
		// Unassign color given to the player
		//UnassignColor((int)player.customProperties["PlayerColorIndex"]);
		
		RefreshPlayerNames();
		
		// Update whether or not the start button is enabled based on the current number of players in the room
		RefreshStartGameButton();
	}

	/// <summary>
	/// Event Listener that is triggered when the player is kicked from the room
	/// This can only happen to clients
	/// </summary>
	private void KickedFromRoom_Event()
	{
		// Tell the MenuManager to transition back to the main menu
		MenuManager.Instance.ShowMainMenu();
	}

	private void MasterClientSwitched_Event(PhotonPlayer newMasterClient)
	{
		// Check to see if the client is the new master client
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			Initialize();
		}
	}
	#endregion

    #region [PunRPC] CALLS
    /// <summary>
	/// PunRPC call that Receives chat messages from any/all clients and displays them in the GUI
	/// </summary>
	/// <param name="playerName">Chatting Player's name</param>
	/// <param name="msg">Chat Message</param>
	[PunRPC]
	private void ReceiveChatMessage(string playerName, string msg)
	{
		// Add the recieved chat to the player's chat area (on the GUI)
		Chat_GUIText.GetComponent<Text>().text += System.Environment.NewLine + "[" + playerName + "]: " + msg;
	}

	/// <summary>
	/// PunRPC call to tell the clients a new level has been selected and to update their available tower choices
	/// </summary>
	/// <param name="levelName">Level name.</param>
	[PunRPC]
	private void NewLevelSelected(string levelName)
	{
		LevelData levelData = GameDataManager.Instance.FindLevelDataByDisplayName(levelName);

		// Display the level name for the clients
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			LevelTitle_GUILabel.GetComponent<Text>().text = "Choose Level:";
		else
			LevelTitle_GUILabel.GetComponent<Text>().text = "Level: " + levelName;

		// Refresh the list of Towers regardless if LevelData can be found. The tower buttons will handle missing data
		LevelLoadoutData = levelData;

		RefreshTowerButtons();
	}

	/// <summary>
	/// PunRPC call to tell the client to start loading the level
	/// </summary>
	[PunRPC]
	private void LoadLevel()
	{
		int playerColorIndex = Random.Range(0, 8);
		SessionManager.Instance.SetPlayerCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "PlayerColorIndex", playerColorIndex } });

		// Record the Loadouts chosen by the player
		PlayerManager.Instance.SetGameLoadOut(new LoadOut(TowerLoadoutData));

		// Start the game
		MenuManager.Instance.ShowStartGame(LevelLoadoutData);
	}
	#endregion
	
	/// <summary>
	/// Refresh the names of all players currently in the room
	/// </summary>
	private void RefreshPlayerNames()
	{
		PhotonPlayer[] playerList = SessionManager.Instance.GetAllPlayersInRoom();
		
		// Reset all names to blank
		for(int i = 0; i < PlayerName_GUIText.Length; i++)
		{
			if(playerList.Length > i)
			{
				// Create a local variable or else the foreach "AddListener" will use a reference to the PhotonPLayer's last reference
				// and will not use unique PhotonPlayers for each (http://stackoverflow.com/questions/25819406/unity-4-6-how-to-stop-clones-sharing-listener)
				PhotonPlayer pp = playerList[i];
				PlayerName_GUIText[i].GetComponent<Text>().text = pp.name;

				// Master Client can kick players
				if(SessionManager.Instance.GetPlayerInfo().isMasterClient && pp.name != PlayerManager.Instance.Username)
				{
					// Need to use "GetChild" because the Child's Component has been set to inactive and is not searchable with "GetComponentInChildren"
					PlayerName_GUIText[i].transform.GetChild(0).gameObject.SetActive(true);
					PlayerName_GUIText[i].GetComponentInChildren<Button>().onClick.AddListener(delegate { KickPlayerButton_Click(pp); });
				}
				else
				{
					// Need to use "GetChild" because the Child's Component has been set to inactive and is not searchable with "GetComponentInChildren"
					// Need to activate the child so that the listeners can be removed (new listeners will be added when the GO is activated
					PlayerName_GUIText[i].transform.GetChild(0).gameObject.SetActive(true);
					PlayerName_GUIText[i].GetComponentInChildren<Button>().onClick.RemoveAllListeners();
					PlayerName_GUIText[i].transform.GetChild(0).gameObject.SetActive(false);
				}
			}
			else
			{
				PlayerName_GUIText[i].GetComponent<Text>().text = "<OPEN>";
				PlayerName_GUIText[i].transform.GetChild(0).gameObject.SetActive(false);
			}
		}
	}
	
	private int AssignColor()
	{
		int index = -1;
		int.TryParse(AvailableColorIndexes[0].ToString(), out index);
		AvailableColorIndexes = AvailableColorIndexes.Remove(0, 1);

		return index;
	}
	
	private void UnassignColor(int colorIndex)
	{
		AvailableColorIndexes = colorIndex.ToString() + AvailableColorIndexes;
	}

	/// <summary>
	/// Refreshes the room details currently being displayed about the room
	/// </summary>
	private void RefreshRoomDetails()
	{
		RoomTitle_GUIText.GetComponent<Text>().text = SessionManager.Instance.GetCurrentRoomInfo().name.Substring(0, SessionManager.Instance.GetCurrentRoomInfo().name.IndexOf("("));;
	}

	/// <summary>
	/// Refreshes the start button in the game to allow/deny the Master Client to start the game
	/// </summary>
	private void RefreshStartGameButton()
	{
		// Only make updates if the player is the Master Client - other players do not see the "Start Game"
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			PhotonPlayer[] playerList = SessionManager.Instance.GetAllPlayersInRoom();

			// Not enough players in the room for this level
			if(playerList.Length < LevelLoadoutData.MinimumPlayers)
			{
				StartGame_GUIButton.GetComponent<Button>().enabled = false;
				StartGame_GUIButton.GetComponentInChildren<Text>().text = "Need Players";
			}
			// Too many players for this level
			else if(playerList.Length > LevelLoadoutData.MaximumPlayers)
			{
				StartGame_GUIButton.GetComponent<Button>().enabled = false;
				StartGame_GUIButton.GetComponentInChildren<Text>().text = "Too Many Players";
			}
			// Eneough players for this level
			else
			{
				StartGame_GUIButton.GetComponent<Button>().enabled = true;
				StartGame_GUIButton.GetComponentInChildren<Text>().text = "Start Game";
			}
		}
	}

	private void RefreshTowerButtons()
	{
		int index = 0;

		// Delete all previously created buttons (and their associated data
		foreach(GameObject button in TowerButtonList)
			Destroy (button);

		TowerButtonList.Clear();
		SelectedTowerButtonList.Clear();
		TowerLoadoutData.Clear();

		if(LevelLoadoutData != null)
		{
			foreach(TowerData towerData in GameDataManager.Instance.TowerDataManager.DataList)
			{
				// Only display this tower if the current level data allows it
				if(LevelLoadoutData.AvailableTowers.Contains(towerData.DisplayName))
				{
					// Create a local variable or else the foreach "AddListener" will use a reference to the foreach towerdata's last reference
					// and will not use unique towerData's for each (http://stackoverflow.com/questions/25819406/unity-4-6-how-to-stop-clones-sharing-listener)
					TowerData td = towerData;

					// Instantiate a button for each tower
					GameObject obj = Instantiate(Resources.Load("GUI/GUI_TowerDetails")) as GameObject;

					obj.GetComponent<TowerButton>().SetTowerData(td);
					obj.GetComponent<Button>().onClick.AddListener(delegate { TowerButton_Click(obj, td); });
					obj.transform.SetParent(this.transform);
					obj.transform.localScale = new Vector3(1, 1, 1);
					obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0 + (128 * index), 200);
					obj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
					obj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
					obj.GetComponent<RectTransform>().localPosition = new Vector3(obj.GetComponent<RectTransform>().localPosition.x, obj.GetComponent<RectTransform>().localPosition.y, 0);
					obj.transform.rotation = new Quaternion(0, 0, 0, 0);

					// Save a reference to this button (so it can be destroyed later)
					TowerButtonList.Add(obj);

					// Select the first X towers by default
					if(index < LevelLoadoutData.MaxTowersPerPlayer)
						TowerButton_Click(obj, td);

					index++;
				}
			}

			// Disable all toggles if the player does not have an option to pick different towers
			/*if(SelectedTowerButtonList.Count <= LevelLoadoutData.MaxTowersPerPlayer)
			{
				foreach(GameObject obj in SelectedTowerButtonList)
					obj.GetComponent<Toggle>().enabled = false;
			}*/
		}
	}

	private void InitiateLevelButtons()
	{
		// Only initiate Level buttons if this is the MASTER CLIENT
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			int index = 0;
			bool previousLevelComplete = true;
			
			foreach (LevelData levelData in GameDataManager.Instance.LevelDataManager.DataList)
			{
				LevelData ld = levelData;

				GameObject obj = Instantiate(Resources.Load("GUI/GUI_LevelDetails")) as GameObject;
				obj.GetComponent<LevelButton>().SetLevelData(ld);
				obj.GetComponent<Button>().onClick.AddListener(delegate { LevelButton_Click(obj, ld); });
				obj.transform.SetParent(this.transform);
				obj.transform.localScale = new Vector3(1, 1, 1);
				obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0 + (128 * index), 360);
				obj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
				obj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
				obj.GetComponent<RectTransform>().localPosition = new Vector3(obj.GetComponent<RectTransform>().localPosition.x, obj.GetComponent<RectTransform>().localPosition.y, 0);
				obj.transform.rotation = new Quaternion(0, 0, 0, 0);

				if(index == 0)
					LevelButton_Click(obj, ld);

				index++;

				/*
				//if(previousLevelComplete)
				//	obj.SetActive(true);
				//else
				//	obj.SetActive(false);

				// select Level1 by default (hacked way)
				if (!LevelLoadoutSelection)
					obj.GetComponent<Toggle>().isOn = true;
					
				if(PlayerManager.Instance.LevelComplete(levelData.DisplayName))
					previousLevelComplete = true;
				else
					previousLevelComplete = false;
				*/
			}
		}
    }

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[RoomDetails_Menu] " + message);
	}
	
	protected void LogError(string message)
	{
		Debug.LogError("[RoomDetails_Menu] " + message);
	}
	#endregion
}