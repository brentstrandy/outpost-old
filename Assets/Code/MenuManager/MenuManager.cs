using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour 
{
	private static MenuManager instance;

	public bool ShowDebugLogs = true;
    public bool ShowRoomInfo = true;

	// Menu Panels
	public GameObject StartMenuPanel;
	public GameObject MainMenuPanel;
	public GameObject CreateGamePanel;
	public GameObject JoinGamePanel;
	public GameObject MatchmakingGamePanel;
	public GameObject MainSettingsPanel;
	public GameObject AudioSettingsPanel;
	public GameObject VideoSettingsPanel;
	public GameObject ControlSettingsPanel;
	public GameObject GameplaySettingsPanel;

	// Menu Join Game Details
	public GameObject JoinGameDetails_Prefab;

	private GameObject CurrentMenuPanel;

    #region Lists of Legacy GUI Menu variables
    //public List<GameObject> Canvases, Panels, MenuHeaders, Buttons;
    //[HideInInspector]
    //public string CurrentMenu, PreviousMenu;
    //[HideInInspector]
    //public GameObject CurrentMenu_GO, PreviousMenu_GO;
    #endregion

    public RoomInfo[] RoomList;
    public Text TextCurrentPlayers;
    private int CurrentPlayers = 0;
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
        #region Lists of Legacy GUI Menu elements
        //CurrentMenu = "Main Menu";

        //Panels = new List<GameObject>(GameObject.FindGameObjectsWithTag("Panel"));
        //// storing these just in case we need customization later on
        //Canvases = new List<GameObject>(GameObject.FindGameObjectsWithTag("Canvas"));
        //MenuHeaders = new List<GameObject>(GameObject.FindGameObjectsWithTag("Menu Header"));
        //Buttons = new List<GameObject>(GameObject.FindGameObjectsWithTag("Button"));

        //// disables all panels except "CurrentMenu"
        //foreach (GameObject panel in Panels)
        //{
        //    if (panel.name != CurrentMenu)
        //    {
        //        panel.SetActive(false);
        //        panel.layer = LayerMask.NameToLayer("UI"); // was "Initial layer"
        //    }
        //}

        //CurrentMenu_GO = Panels.Find(panel => panel.name == CurrentMenu);

        //if (ShowDebugLogs)
        //{
        //    foreach (GameObject canvas in Canvases)
        //        this.Log("  [CANVAS]" + canvas.name);
        //    foreach (GameObject panel in Panels)
        //        this.Log("  [PANEL] " + panel.name);
        //    foreach (GameObject header in MenuHeaders)
        //        this.Log("  [MENU HEADER] " + header.name);
        //    foreach (GameObject button in Buttons)
        //        this.Log("  [BUTTON] " + button.name);
        //}
        #endregion

        #region Audio
        string soundEffectsPath = "Audio/Effects/";
        ShortClick = Resources.Load(soundEffectsPath + "short_click_01") as AudioClip;
        LongClick = Resources.Load(soundEffectsPath + "long_click_01") as AudioClip;
        #endregion

		// Set the current menu panel as the start menu
		SetCurrentMenuPanel(StartMenuPanel);

        #region Network Session
        // Track events in order to react to Session Manager events as they happen
		SessionManager.Instance.OnSMConnected += ConnectedToNetwork;
		SessionManager.Instance.OnSMJoinedLobby += JoinedLobby;
		SessionManager.Instance.OnSMConnectionFail += DisconnectedFromNetwork;
		SessionManager.Instance.OnSMCreatedRoom += RoomCreated;
        #endregion
    }

    void Update ()
    {
        // update number of players in room
        //if (ShowRoomInfo && Room != null)
        //{
        //    if (Room.playerCount != CurrentPlayers)
        //        CurrentPlayers = Room.playerCount;
        //}
    }

    #region USED WITH LEGACY GUI SCENE
    //public void ButtonClick(string menuName)
    //{
    //    //ButtonSound();
    //    MenuChange(menuName);
    //}

    //public void ButtonSound(string menuName)
    //{
    //    // create system for playing a short or long click ("simple selection" vs "critical selection")
    //    if (true)
    //        PlayOneShotSound(ShortClick);
    //    else
    //        PlayOneShotSound(LongClick);
    //}

    //public void PlayOneShotSound(AudioClip clip)
    //{
    //    if (clip)
    //        audio.PlayOneShot(clip);
    //    else
    //        this.LogError("Audio clip not available.");
    //}

    //public void MenuChange(string menuName)
    //{
    //    string tempCurrentMenu = CurrentMenu;
    //    GameObject tempCurrentMenu_GO;

    //    //if (menuName == "Create Game")
    //    //{
    //    //    Application.LoadLevel("MainGame");
    //    //}

    //    if (menuName != "Back")
    //    {
    //        // if the next menu item (menuName) DOES exist...
    //        if ((tempCurrentMenu_GO = Panels.Find(panel => panel.name == menuName)) != null)
    //        {
    //            // set the previous menu
    //            PreviousMenu_GO = CurrentMenu_GO;
    //            PreviousMenu = CurrentMenu;
    //            // set the current menu
    //            CurrentMenu_GO = tempCurrentMenu_GO;
    //            CurrentMenu = menuName;

    //            PreviousMenu_GO.SetActive(false);
    //            CurrentMenu_GO.SetActive(true);
    //        }
    //        // if the next menu item (menuName) DOES NOT exist...
    //        else
    //        {
    //            this.LogError(menuName + " not available.");
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("         Current Menu: " + CurrentMenu_GO.name);
    //        CurrentMenu = PreviousMenu;
    //        CurrentMenu_GO.SetActive(false);
    //        PreviousMenu_GO.SetActive(true);
    //    }
    //}
    #endregion

    public void PlayShortClick()
    {
        audio.PlayOneShot(ShortClick);
    }

    public void PlayLongClick()
    {
        audio.PlayOneShot(LongClick);
    }

    public void GatherNewRoomInfo()
    {
        //Room = SessionManager.Instance.GetCurrentRoomInfo();
        ShowRoomInfo = true;
    }

	#region OnClick
	public void StartSession_Click()
	{
		PlayShortClick();

		// Maybe GUI should have an initial "Start" button. When pressed, the game attampts to connect to the network
        // Like a pre-screen that waits for user input. After user presses something the attempt to connect to network occurs
		SessionManager.Instance.StartSession();
	}

	public void CreateGame_Click()
	{
		PlayShortClick();

		// Ask the session manager to create a new room
		SessionManager.Instance.CreateRoom("", new RoomOptions() { isVisible = true, isOpen = true, maxPlayers = 8 }, TypedLobby.Default);
	}

	public void JoinGame_Click()
	{
		PlayShortClick();

		// Ask the session manager to join the default lobby
		SessionManager.Instance.JoinLobby();
	}

	public void JoinRoom_Click()
	{

	}

	public void MatchmakingGame_Click()
	{
		PlayShortClick();

		SetCurrentMenuPanel(MatchmakingGamePanel);
	}

	public void MainSettings_Click()
	{
		PlayShortClick();

		SetCurrentMenuPanel(MainSettingsPanel);
	}

	public void MainMenu_Click()
	{
		PlayLongClick();

		// When returning to the main menu leave any room or lobby
		SessionManager.Instance.LeaveRoom();
		SessionManager.Instance.LeaveLobby();

		SetCurrentMenuPanel(MainMenuPanel);
	}
	#endregion

	/// <summary>
	/// Called whenever the SessionManager connects to the network
	/// </summary>
	private void ConnectedToNetwork()
	{
		SetCurrentMenuPanel(MainMenuPanel);
	}

	/// <summary>
	/// Called whenever the SessionManager joins a lobby
	/// </summary>
	private void JoinedLobby()
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
			obj.GetComponent<Button>().onClick.AddListener(() => JoinRoom_Click());
			obj.transform.parent = JoinGamePanel.transform;
			index++;
		}
	}

	/// <summary>
	/// Called whenever the SessionManager is disconnected from the network
	/// </summary>
	/// <param name="cause">Cause.</param>
	private void DisconnectedFromNetwork(DisconnectCause cause)
	{
        ShowRoomInfo = false;

		SetCurrentMenuPanel(StartMenuPanel);
	}

	/// <summary>
	/// Called whenever the SessionManager creates a room
	/// </summary>
	private void RoomCreated()
	{
		SetCurrentMenuPanel(CreateGamePanel);
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
