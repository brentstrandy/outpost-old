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
	private List<GameObject> TowerLoadoutSelections;
    private GameObject LevelLoadoutSelection;
    /// <summary>
	/// Tracks TowerData for each tower that is selected for the loadout
	/// </summary>
	private List<TowerData> TowerLoadoutData;
    /// Tracks LevelData for the level that is selected for the Loadout
    /// </summary>
    private LevelData LevelLoadoutData;
    private bool LevelSelected;

	private PhotonView ObjPhotonView;

	public void Awake()
	{
		TowerLoadoutSelections = new List<GameObject>();
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

		// Add Tower Butttons for each Tower
		InitiateTowerButtons();

        // Add Level Buttons for each Level
        InitiateLevelButtons();

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
		    ObjPhotonView.RPC ("StartGame", PhotonTargets.All, null);
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
			TowerLoadoutSelections.Remove(towerButton);
			TowerLoadoutData.Remove(towerData);
		}
		else
		{
			// Add the new tower to the loadout
			TowerLoadoutSelections.Add(towerButton);
			TowerLoadoutData.Add(towerData);

			if(TowerLoadoutSelections.Count > 2)
			{
				// Untoggle the last selected tower (This will trigger the event to call this function again)
				TowerLoadoutSelections[0].GetComponent<Toggle>().isOn = false;
			}
		}
	}

    /// <summary>
    /// Click event called when the player selects a level within this menu
    /// </summary>	
    public void LevelButton_Click(GameObject levelButton, LevelData levelData)
    {
        // If the toggle button was turned off
        if (levelButton.GetComponent<Toggle>().isOn == false)
            LevelSelected = false;
        // If the toggle button was turned on
        else
        {
            if (LevelLoadoutSelection != null)
            {
                // Turns off previously checked toggle buttons
                if (levelButton.GetComponent<Toggle>().isOn == true && levelButton != LevelLoadoutSelection)
                    LevelLoadoutSelection.GetComponent<Toggle>().isOn = false;
            }

            // Sets level to be loaded
            LevelLoadoutData = levelData;
            LevelSelected = true;
        }

        // Updates currently toggled-on button
        LevelLoadoutSelection = levelButton;
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
	void ReceiveChatMessage(string playerName, string msg)
	{
		// Add the recieved chat to the player's chat area (on the GUI)
		Chat_GUIText.GetComponent<Text>().text += System.Environment.NewLine + "[" + playerName + "]: " + msg;
	}

	/// <summary>
	/// RPC call to tell the client the game is starting
	/// </summary>
	[RPC]
	void StartGame()
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

	private void InitiateTowerButtons()
	{
		int index = 0;
        
		foreach(TowerData towerData in GameDataManager.Instance.TowerDataMngr.TowerDataList)
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
			obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-160 + (60 * index), 50);
			obj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
			obj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
			obj.GetComponent<RectTransform>().localPosition = new Vector3(obj.GetComponent<RectTransform>().localPosition.x, obj.GetComponent<RectTransform>().localPosition.y, 0);
			obj.transform.rotation = new Quaternion(0, 0, 0, 0);
			// Select the first two towers by default
			if(index < 2)
			{
				obj.GetComponent<Toggle>().isOn = true;
			}
			else
				obj.GetComponent<Toggle>().isOn = false;

			index++;
		}
	}

    private void InitiateLevelButtons()
    {
        int index = 0;
        
        foreach (LevelData levelData in GameDataManager.Instance.LevelDataMngr.LevelDataList)
        {
            LevelData ld = levelData;
            
            // instantiate a button for each level
            GameObject obj = Instantiate(Resources.Load("GUI_LevelDetails")) as GameObject;
            obj.GetComponentInChildren<Text>().text = levelData.DisplayName;
            obj.GetComponent<Toggle>().onValueChanged.AddListener(delegate { LevelButton_Click(obj, ld); });
            obj.transform.SetParent(this.transform);
            obj.transform.localScale = new Vector3(1, 1, 1);
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(160 + (70 * index), 50);
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