using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class RoomDetails_Menu : MonoBehaviour
{
	/* Player Color Picking
	* The Master Client is initially assigned a color when the room is created
	* When a client joins, the Master Client assigns a color to that client (triggering the Event "PlayerCustPropUpdated_Event")
	* When "PlayerCustPropUpdated_Event" is triggered all players update their display
	* When a client changes their color the MasterClient is notified, validates, and updates the player's color (triggering the Event "PlayerCustPropUpdated_Event")
	* When the Master Client is switched to a different client, the new Master Client assumes the role of the old Master Client
	*/

    public bool ShowDebugLogs = true;

    // Handles to GUI objects
    public GameObject[] PlayerName_GUIText;

    public GameObject RoomTitle_GUIText;
    public GameObject Chat_GUIText;
    public GameObject SendChat_GUIInput;
    public GameObject StartGame_GUIButton;
    public GameObject LevelTitle_GUILabel;
	public Dropdown PlayerColor_GUIDropdown;

	public TowerLoadoutButtonManager TowerLoadoutButtonMngr;

    /// <summary>
    /// Tracks GUI objects for each tower that is selected for the Loadout
    /// </summary>
    //private List<GameObject> SelectedTowerButtonList;

    //private List<GameObject> TowerButtonList;
    private GameObject LevelLoadoutSelection;

    /// <summary>
    /// Tracks LevelData for the level that is selected for the Loadout
    /// </summary>
    private LevelData LevelLoadoutData;

    private bool LevelSelected;
	private int[] PlayerColorIndexes;

    private PhotonView ObjPhotonView;

    public void Awake()
    {
		PlayerColorIndexes = new int[PlayerColors.colors.Length];
		// Initialize all player colors
		for(int i = 0; i < PlayerColors.colors.Length; i++)
			PlayerColorIndexes[i] = -1;

		SetPlayerColor(GetNextColorIndex(), SessionManager.Instance.GetPlayerInfo());

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
		SessionManager.Instance.OnSMPlayerPropertiesChanged += PlayerCustPropUpdated_Event;
		SessionManager.Instance.OnSMRoomPropertiesChanged += RoomCustPropUpdated_Event;

        Initialize();
    }

    /// <summary>
    /// OnDisaple is called when the menu is triggered to be hidden
    /// </summary>
    private void OnDisable()
    {
        // Remove listeners for all applicable events (these listeners are added again when the menu is shown)
		if(SessionManager.Instance != null)
		{
			SessionManager.Instance.OnSMPlayerJoinedRoom -= PlayerJoinedRoom_Event;
        	SessionManager.Instance.OnSMPlayerLeftRoom -= PlayerLeftRoom_Event;
        	SessionManager.Instance.OnSMLeftRoom -= KickedFromRoom_Event;
			SessionManager.Instance.OnSMSwitchMaster -= MasterClientSwitched_Event;
			SessionManager.Instance.OnSMPlayerPropertiesChanged -= PlayerCustPropUpdated_Event;
		}

        // Clear chat history so the next room will start with a clear chat area
        Chat_GUIText.GetComponent<Text>().text = "";
    }

    private void Initialize()
    {
        // See if the player is currently the room owner or just joining
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            StartGame_GUIButton.SetActive(true);
            SessionManager.Instance.SetRoomVisibility(true);
        }
        else
            StartGame_GUIButton.SetActive(false);

		// Initialize all player colors
		for(int i = 0; i < PlayerColors.colors.Length; i++)
			PlayerColorIndexes[i] = -1;
		foreach(PhotonPlayer pp in SessionManager.Instance.GetAllPlayersInRoom())
		{
			if(pp.customProperties["PlayerColorIndex"] != null)
				PlayerColorIndexes[(int)pp.customProperties["PlayerColorIndex"]] = pp.ID;
		}

        // Immediately refresh the player name list to show the host
        RefreshPlayerNames();

        // Immediately refresh the room details
        RefreshRoomDetails();

        // Add Level Buttons for each Level (this needs to be done before the tower buttons because the towers depend on the LevelData)
        InitiateLevelButtons();

		// Get the current level loadout data
		if(LevelLoadoutData == null)
		{
			if(SessionManager.Instance.GetCurrentRoomInfo().customProperties["L_ID"] != null)
				NewLevelSelected((int)SessionManager.Instance.GetCurrentRoomInfo().customProperties["L_ID"]);
			else
				LogError("Unable to find Level ID");
		}
		else
			NewLevelSelected(LevelLoadoutData.LevelID);
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
            if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
				SessionManager.Instance.SetRoomVisibility(false);
				//SessionManager.Instance.SetRoomVisibility(false);
            
			// Tell all the clients to load the level
            ObjPhotonView.RPC("LoadLevel", PhotonTargets.All, null);
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
        if (SendChat_GUIInput.GetComponent<InputField>().text != "")
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
        /*if (towerButton.GetComponent<TowerLoadoutButton>().Selected)
        {
            SelectedTowerButtonList.Remove(towerButton);
            TowerLoadoutData.Remove(towerData);

            // Tell the button to unselect itself
            towerButton.GetComponent<TowerLoadoutButton>().UnselectTower();
        }
        else
        {
            // Add the new tower to the loadout
            SelectedTowerButtonList.Add(towerButton);
            TowerLoadoutData.Add(towerData);

            // Tell the button to select itself
            towerButton.GetComponent<TowerLoadoutButton>().SelectTower();
        }*/
    }

    /// <summary>
    /// Click event called when the player selects a level within this menu
    /// </summary>
    //public void LevelButton_Click(GameObject levelButton, LevelData levelData)
    private void LevelButton_Click(GameObject levelButton, LevelData levelData)
    {
        // Unselect the previous level (if there was a previously selected level)
        if (LevelLoadoutSelection != null)
            LevelLoadoutSelection.GetComponent<LevelButton>().UnselectLevel();

        // Updates currently selected level
        LevelLoadoutSelection = levelButton;

        // Tell the button to select itself
        LevelLoadoutSelection.GetComponent<LevelButton>().SelectLevel();

		// Inform players in the room a new level has been selected (This also saves the level ID for others to see in the lobby)
		SessionManager.Instance.SetRoomCustomProperties(new Hashtable { { "L_ID", levelData.LevelID } });

        // Sets level to be loaded
        LevelLoadoutData = levelData;
        LevelSelected = true;

        // Update the available Towers to choose from
        //RefreshTowerButtons();

        // Update whether or not the start button is enabled based on the current number of players in the room
        RefreshStartGameButton();
    }

    public void KickPlayerButton_Click(PhotonPlayer player)
    {
        SessionManager.Instance.KickPlayer(player);
    }

	public void PlayerColor_Click()
	{
		if(!PlayerColor_GUIDropdown.captionText.text.Contains("<!>"))
			ObjPhotonView.RPC("SetPlayerColor", PhotonTargets.MasterClient, System.Array.FindIndex(PlayerColors.names, x => x == PlayerColor_GUIDropdown.captionText.text.ToString()));
	}

    #endregion ON CLICK

    #region EVENTS

    /// <summary>
    /// Event Listener that is triggered when a player joins the room
    /// </summary>
    /// <param name="player">Player Data</param>
    private void PlayerJoinedRoom_Event(PhotonPlayer player)
    {
		// Immediately set the player's color
		SetPlayerColor(GetNextColorIndex(), player);

		// New player has joined, update the list of current players
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
        UnassignColorIndex((int)player.customProperties["PlayerColorIndex"]);

		// Player has left the room, update the list of current players
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

	/// <summary>
	/// Event listener that is triggered when any player updates a custom property
	/// </summary>
	/// <param name="playerAndUpdatedProps">Player and updated properties.</param>
	private void PlayerCustPropUpdated_Event(object[] playerAndUpdatedProps)
	{
		// The client needs to save which colors have been picked so they know
		if(!SessionManager.Instance.GetPlayerInfo().isMasterClient)
			AssignColorIndex((int)((PhotonPlayer)playerAndUpdatedProps[0]).customProperties["PlayerColorIndex"], ((PhotonPlayer)playerAndUpdatedProps[0]).ID);

		this.RefreshPlayerColorDropdown();
		this.RefreshPlayerNames();
	}

	private void RoomCustPropUpdated_Event(Hashtable propertiesThatChanged)
	{
		// Check to see if the level changed
		if(propertiesThatChanged["L_ID"] != null)
		{
			// Only select a new level if the selected level is new
			if(LevelLoadoutData != null && LevelLoadoutData.LevelID != (int)propertiesThatChanged["L_ID"])
				NewLevelSelected((int)propertiesThatChanged["L_ID"]);
		}
	}

    private void MasterClientSwitched_Event(PhotonPlayer newMasterClient)
    {
        // Check to see if the client is the new master client
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            Initialize();
        }
    }

    #endregion EVENTS

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
    /// PunRPC call to tell the client to start loading the level
    /// </summary>
    [PunRPC]
    private void LoadLevel()
    {
        // Record the Loadouts chosen by the player
		PlayerManager.Instance.CurPlayer.SetGameLoadOut(new LoadOut(TowerLoadoutButtonMngr.GetSelectedTowerList()));

        // Start the game
        MenuManager.Instance.ShowStartGame(LevelLoadoutData);
    }

	[PunRPC]
	private void SetPlayerColor(int colorIndex, PhotonMessageInfo msgInfo)
	{
		SetPlayerColor(colorIndex, msgInfo.sender);
	}

    #endregion

	private void SetPlayerColor(int colorIndex, PhotonPlayer player)
	{
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Ensure the color is available to be picked
			if(PlayerColorIndexes[colorIndex] == -1)
			{
				AssignColorIndex(colorIndex, player.ID);

				// Set the player's color, triggering the Event "PlayerCustPropUpdated_Event"
				player.SetCustomProperties(new Hashtable() { { "PlayerColorIndex", colorIndex } });
			}
		}
	}

	/// <summary>
	/// A new level has been selected. Update the available tower choices
	/// </summary>
	/// <param name="levelID">Level ID.</param>
	private void NewLevelSelected(int levelID)
	{
		LevelData levelData = GameDataManager.Instance.FindLevelDataByLevelID(levelID);

		// Display the level name for the clients
		if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
			LevelTitle_GUILabel.GetComponent<Text>().text = "Choose Level:";
		else
			LevelTitle_GUILabel.GetComponent<Text>().text = "Level: " + levelData.DisplayName;

		// Refresh the list of Towers regardless if LevelData can be found. The tower buttons will handle missing data
		LevelLoadoutData = levelData;

		RefreshTowerButtons();
	}

	private void RefreshPlayerColorDropdown()
	{
		string colorTitle;
		PlayerColor_GUIDropdown.ClearOptions();

		for(int i = 0; i < PlayerColors.colors.Length; i++)
		{
			if(PlayerColorIndexes[i] != -1)
				colorTitle = "<!> " + PlayerColors.names[i].ToString();
			else
				colorTitle = PlayerColors.names[i].ToString();

			PlayerColor_GUIDropdown.options.Add(new Dropdown.OptionData(colorTitle));
		}
	}

    /// <summary>
    /// Refresh the names of all players currently in the room
    /// </summary>
    private void RefreshPlayerNames()
    {
        PhotonPlayer[] playerList = SessionManager.Instance.GetAllPlayersInRoom();

        // Reset all names to blank
        for (int i = 0; i < PlayerName_GUIText.Length; i++)
        {
            if (playerList.Length > i)
            {
                // Create a local variable or else the foreach "AddListener" will use a reference to the PhotonPLayer's last reference
                // and will not use unique PhotonPlayers for each (http://stackoverflow.com/questions/25819406/unity-4-6-how-to-stop-clones-sharing-listener)
                PhotonPlayer pp = playerList[i];
                PlayerName_GUIText[i].GetComponent<Text>().text = pp.name;

				if(pp.customProperties["PlayerColorIndex"] != null)
					PlayerName_GUIText[i].GetComponent<Text>().color = PlayerColors.colors[(int)pp.customProperties["PlayerColorIndex"]];
				else
					PlayerName_GUIText[i].GetComponent<Text>().color = Color.white;

                // Master Client can kick players
                if (SessionManager.Instance.GetPlayerInfo().isMasterClient && pp.name != PlayerManager.Instance.CurPlayer.Username)
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
				PlayerName_GUIText[i].GetComponent<Text>().color = Color.white;
                PlayerName_GUIText[i].transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

	/// <summary>
	/// Simply get the next available color - do NOT assign the color yet
	/// </summary>
	/// <returns>The next color index.</returns>
	private int GetNextColorIndex()
	{
		int index = -1;

		for(int i = 0; i < PlayerColorIndexes.Length; i++)
		{
			if(PlayerColorIndexes[i] == -1)
			{
				index = i;
				i = PlayerColorIndexes.Length;
			}
		}

		return index;
	}

	private void AssignColorIndex(int colorIndex, int playerID)
	{
		// Remove the old index before assigning it in a new place
		for(int i = 0; i < PlayerColorIndexes.Length; i++)
		{
			if(PlayerColorIndexes[i] == playerID)
				PlayerColorIndexes[i] = -1;
		}

		PlayerColorIndexes[colorIndex] = playerID;
	}

    private void UnassignColorIndex(int colorIndex)
    {
		PlayerColorIndexes[colorIndex] = -1;
    }

    /// <summary>
    /// Refreshes the room details currently being displayed about the room
    /// </summary>
    private void RefreshRoomDetails()
    {
        RoomTitle_GUIText.GetComponent<Text>().text = SessionManager.Instance.GetCurrentRoomInfo().name.Substring(0, SessionManager.Instance.GetCurrentRoomInfo().name.IndexOf("(")); ;

		RefreshPlayerColorDropdown();
    }

    /// <summary>
    /// Refreshes the start button in the game to allow/deny the Master Client to start the game
    /// </summary>
    private void RefreshStartGameButton()
    {
        // Only make updates if the player is the Master Client - other players do not see the "Start Game"
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            PhotonPlayer[] playerList = SessionManager.Instance.GetAllPlayersInRoom();

            // Not enough players in the room for this level
            if (playerList.Length < LevelLoadoutData.MinimumPlayers)
            {
                StartGame_GUIButton.GetComponent<Button>().enabled = false;
                StartGame_GUIButton.GetComponentInChildren<Text>().text = "Need Players";
            }
            // Too many players for this level
            else if (playerList.Length > LevelLoadoutData.MaximumPlayers)
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
		TowerLoadoutButtonMngr.UpdateAvailableTowers(LevelLoadoutData);

		/*
        int index = 0;

        // Delete all previously created buttons (and their associated data
        foreach (GameObject button in TowerButtonList)
            Destroy(button);

        TowerButtonList.Clear();
        SelectedTowerButtonList.Clear();
        TowerLoadoutData.Clear();

        if (LevelLoadoutData != null)
        {
            foreach (TowerData towerData in GameDataManager.Instance.TowerDataManager.DataList)
            {
                // Only display this tower if the current level data allows it
                if (LevelLoadoutData.AvailableTowers.Contains(towerData.DisplayName))
                {
                    // Create a local variable or else the foreach "AddListener" will use a reference to the foreach towerdata's last reference
                    // and will not use unique towerData's for each (http://stackoverflow.com/questions/25819406/unity-4-6-how-to-stop-clones-sharing-listener)
                    TowerData td = towerData;

                    // Instantiate a button for each tower
                    GameObject obj = Instantiate(Resources.Load("GUI/GUI_TowerDetails")) as GameObject;

                    obj.GetComponent<TowerLoadoutButton>().SetTowerData(td);
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
                    if (index < LevelLoadoutData.MaxTowersPerPlayer)
                        TowerButton_Click(obj, td);

                    index++;
                }
            }
        }*/
    }

    private void InitiateLevelButtons()
    {
        // Only initiate Level buttons if this is the MASTER CLIENT
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            int index = 0;
            //bool previousLevelComplete = true;

            // First, order levels by ID
            GameDataManager.Instance.LevelDataManager.SortDataList();

            foreach (LevelData levelData in GameDataManager.Instance.LevelDataManager.DataList)
            {
                LevelData ld = levelData;

                GameObject obj = Instantiate(Resources.Load("GUI/GUI_LevelDetails")) as GameObject;
                obj.GetComponent<LevelButton>().SetLevelData(ld);
                obj.GetComponent<Button>().onClick.AddListener(delegate { LevelButton_Click(obj, ld); });
                obj.transform.SetParent(this.transform);
                obj.transform.localScale = new Vector3(1, 1, 1);
                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100 + (128 * index), 360);
                obj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
                obj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
                obj.GetComponent<RectTransform>().localPosition = new Vector3(obj.GetComponent<RectTransform>().localPosition.x, obj.GetComponent<RectTransform>().localPosition.y, 0);
                obj.transform.rotation = new Quaternion(0, 0, 0, 0);

                if (index == 0)
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
        if (ShowDebugLogs)
            Debug.Log("[RoomDetails_Menu] " + message);
    }

    protected void LogError(string message)
    {
        Debug.LogError("[RoomDetails_Menu] " + message);
    }

    #endregion MessageHandling
}