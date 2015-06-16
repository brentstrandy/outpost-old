using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RoomDetails_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	// Handles to GUI objects
	public GameObject[] PlayerName_GUIText;
	public GameObject RoomTitle_GUIInput;
	public GameObject Chat_GUIText;
	public GameObject SendChat_GUIInput;
	public GameObject StartGame_GUIButton;

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

		// Immediately refresh the player name list to show the host
        RefreshPlayerNames();

		// Immediately refresh the room details
		RefreshRoomDetails();

		// Add Level Buttons for each Level (this needs to be done before the tower buttons because the towers depend on the LevelData)
		InitiateLevelButtons();

		// Add Tower Butttons for each Tower
		RefreshTowerButtons();

		// See if the player is currently the room owner or just joining
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			RoomTitle_GUIInput.GetComponent<InputField>().enabled = true;
			StartGame_GUIButton.SetActive(true);
		}
		else
		{
			RoomTitle_GUIInput.GetComponent<InputField>().enabled = false;
			StartGame_GUIButton.SetActive(false);
		}
	}

	/// <summary>
	/// OnDisaple is called when the menu is triggered to be hidden
	/// </summary>
	private void OnDisable()
	{
		// Remove listeners for all applicable events (these listeners are added again when the menu is shown)
		SessionManager.Instance.OnSMPlayerJoinedRoom -= PlayerJoinedRoom_Event;
		SessionManager.Instance.OnSMPlayerLeftRoom -= PlayerLeftRoom_Event;

		// Clear chat history so the next room will start with a clear chat area
		Chat_GUIText.GetComponent<Text>().text = "";
	}
	
	#region ON CLICK
	/// <summary>
	/// Used by the GUI system to start the game when the Start button is Clicked
	/// </summary>
	public void StartGame_Click()
	{
        // only starts game if the user has selected a level
        if (LevelSelected)
		    ObjPhotonView.RPC ("LoadLevel", PhotonTargets.All, null);
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

	// I don't think I want this (Brent Strandy 2/19/15)
	public void RoomTitle_AfterUpdate()
	{
		// Set the custom properties of a room
		//Room.SetCustomProperties(Hashtable propertiesToSet);
		SessionManager.Instance.GetCurrentRoomInfo().name = RoomTitle_GUIInput.GetComponent<InputField>().text;
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
		// If the toggle was turned off, then remove it from the Loadout
		if(towerButton.GetComponent<Toggle>().isOn == false)
		{
			SelectedTowerButtonList.Remove(towerButton);
			TowerLoadoutData.Remove(towerData);
		}
		else
		{
			// Add the new tower to the loadout
			SelectedTowerButtonList.Add(towerButton);
			TowerLoadoutData.Add(towerData);

			if(SelectedTowerButtonList.Count > LevelLoadoutData.MaxTowersPerPlayer)
			{
				// Untoggle the last selected tower (This will trigger the event to call this function again)
				SelectedTowerButtonList[0].GetComponent<Toggle>().isOn = false;
			}
		}
	}

    /// <summary>
    /// Click event called when the player selects a level within this menu
    /// </summary>	
    public void LevelButton_Click(GameObject levelButton, LevelData levelData)
    {
		// Whenever the toggle is TRUE that means the toggle has JUST been changed to true.
		if(levelButton.GetComponent<Toggle>().isOn == true)
		{
			// Tell all other clients that a new level has been selected
			ObjPhotonView.RPC("NewLevelSelected", PhotonTargets.Others, levelData.DisplayName);

            // Sets level to be loaded
            LevelLoadoutData = levelData;
            LevelSelected = true;

			// Updates currently toggled-on button
			LevelLoadoutSelection = levelButton;

			// Update the available Towers to choose from
			RefreshTowerButtons();
        }
    }
	#endregion
	
	#region EVENTS
	/// <summary>
	/// Event Listener that is triggered when a player joins the room
	/// </summary>
	/// <param name="player">Player Data</param>
	private void PlayerJoinedRoom_Event(PhotonPlayer player)
	{
		RefreshPlayerNames();
	}

	/// <summary>
	/// Event Listener that is triggered when a player leaves the room
	/// </summary>
	/// <param name="player">Player.</param>
	private void PlayerLeftRoom_Event(PhotonPlayer player)
	{
		RefreshPlayerNames();
	}
	#endregion

	#region RPC CALLS
	/// <summary>
	/// RPC call that Receives chat messages from any/all clients and displays them in the GUI
	/// </summary>
	/// <param name="playerName">Chatting Player's name</param>
	/// <param name="msg">Chat Message</param>
	[RPC]
	private void ReceiveChatMessage(string playerName, string msg)
	{
		// Add the recieved chat to the player's chat area (on the GUI)
		Chat_GUIText.GetComponent<Text>().text += System.Environment.NewLine + "[" + playerName + "]: " + msg;
	}

	/// <summary>
	/// RPC call to tell the clients a new level has been selected and to update their available tower choices
	/// </summary>
	/// <param name="levelName">Level name.</param>
	[RPC]
	private void NewLevelSelected(string levelName)
	{;
		LevelData levelData = GameDataManager.Instance.FindLevelDataByDisplayName(levelName);
	
		// Refresh the list of Towers regardless if LevelData can be found. The tower buttons will handle missing data
		LevelLoadoutData = levelData;
		RefreshTowerButtons();
	}

	/// <summary>
	/// RPC call to tell the client to start loading the level
	/// </summary>
	[RPC]
	private void LoadLevel()
	{
		// Record the Loadouts chosen by the player
		Player.Instance.SetGameLoadOut(new LoadOut(TowerLoadoutData));

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
				PlayerName_GUIText[i].GetComponent<Text>().text = playerList[i].name;
			else
				PlayerName_GUIText[i].GetComponent<Text>().text = "<OPEN>";
		}
	}

	/// <summary>
	/// Refreshes the room details currently being displayed about the room
	/// </summary>
	private void RefreshRoomDetails()
	{
		RoomTitle_GUIInput.GetComponent<InputField>().text = SessionManager.Instance.GetCurrentRoomInfo().name;
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
			foreach(TowerData towerData in GameDataManager.Instance.TowerDataMngr.DataList)
			{
				// Only display this tower if the current level data allows it
				if(LevelLoadoutData.AvailableTowers.Contains(towerData.DisplayName))
				{
					// Create a local variable or else the foreach "AddListener" will use a reference to the foreach towerdata's last reference
					// and will not use unique towerData's for each (http://stackoverflow.com/questions/25819406/unity-4-6-how-to-stop-clones-sharing-listener)
					TowerData td = towerData;
					// Instantiate a button for each tower
					GameObject obj = Instantiate(Resources.Load("GUI_TowerDetails")) as GameObject;
					obj.GetComponentInChildren<Text>().text = towerData.DisplayName + "($" + towerData.InstallCost.ToString() + ")";
					obj.GetComponent<Toggle>().onValueChanged.AddListener(delegate{TowerButton_Click(obj, td);});
					obj.transform.SetParent(this.transform);
					obj.transform.localScale = new Vector3(1, 1, 1);
					obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(160 + (60 * index), 50);
					obj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
					obj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
					obj.GetComponent<RectTransform>().localPosition = new Vector3(obj.GetComponent<RectTransform>().localPosition.x, obj.GetComponent<RectTransform>().localPosition.y, 0);
					obj.transform.rotation = new Quaternion(0, 0, 0, 0);

					// Save a reference to this button (so it can be destroyed later)
					TowerButtonList.Add(obj);

					// Select the first X towers by default
					if(index < LevelLoadoutData.MaxTowersPerPlayer)
						obj.GetComponent<Toggle>().isOn = true;
					else
						obj.GetComponent<Toggle>().isOn = false;

					index++;
				}
			}

			// Disable all toggles if the player does not have an option to pick different towers
			if(SelectedTowerButtonList.Count <= LevelLoadoutData.MaxTowersPerPlayer)
			{
				foreach(GameObject obj in SelectedTowerButtonList)
					obj.GetComponent<Toggle>().enabled = false;
			}
		}
	}

    private void InitiateLevelButtons()
    {
		// Only initiate Level buttos if this is the MASTER CLIENT
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
	        int index = 0;
			// Create a toggle group to grop all the toggles and automatically enforce only one selection
			GameObject toggleGroup = new GameObject();
			toggleGroup.AddComponent<ToggleGroup>();
			toggleGroup.GetComponent<ToggleGroup>().allowSwitchOff = true;

	        foreach (LevelData levelData in GameDataManager.Instance.LevelDataMngr.DataList)
	        {
				LevelData ld = levelData;

	            // instantiate a button for each level
	            GameObject obj = Instantiate(Resources.Load("GUI_LevelDetails")) as GameObject;
	            obj.GetComponentInChildren<Text>().text = levelData.DisplayName;
	            obj.GetComponent<Toggle>().onValueChanged.AddListener(delegate { LevelButton_Click(obj, ld); });
	            obj.transform.SetParent(this.transform);
	            obj.transform.localScale = new Vector3(1, 1, 1);
				obj.GetComponent<Toggle>().group = toggleGroup.GetComponent<ToggleGroup>();
	            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-180 + (70 * index), 50);
	            obj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
	            obj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
	            obj.GetComponent<RectTransform>().localPosition = new Vector3(obj.GetComponent<RectTransform>().localPosition.x, obj.GetComponent<RectTransform>().localPosition.y, 0);
	            obj.transform.rotation = new Quaternion(0, 0, 0, 0);

	            // select Level1 by default (hacked way)
	            if (!LevelLoadoutSelection)
	                obj.GetComponent<Toggle>().isOn = true;

	            index++;
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
		if(ShowDebugLogs)
			Debug.LogError("[RoomDetails_Menu] " + message);
	}
	#endregion
}