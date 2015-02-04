using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour 
{
	public bool ShowDebugLogs = true;
    public bool ShowRoomInfo = true;
	private static MenuManager instance;

    #region Lists of Legacy GUI Menu variables
    //public List<GameObject> Canvases, Panels, MenuHeaders, Buttons;
    //[HideInInspector]
    //public string CurrentMenu, PreviousMenu;
    //[HideInInspector]
    //public GameObject CurrentMenu_GO, PreviousMenu_GO;
    #endregion
    public RoomInfo Room;
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

        #region Photon
        // Track events in order to react to Session Manager events as they happen
		SessionManager.Instance.OnSMConnected += ConnectedToNetwork;
		SessionManager.Instance.OnSMConnectionFail += DisconnectedFromNetwork;
		SessionManager.Instance.OnSMCreatedRoom += RoomCreated;
		SessionManager.Instance.StartSession();
        #endregion
    }

    void Update ()
    {
        // update number of players in room
        if (ShowRoomInfo && Room != null)
        {
            if (Room.playerCount != CurrentPlayers)
                CurrentPlayers = Room.playerCount;
        }
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
        Room = SessionManager.Instance.GetCurrentRoomInfo();
        ShowRoomInfo = true;
    }

	public void StartSession_Click()
	{
		// Maybe GUI should have an initial "Start" button. When pressed, the game attampts to connect to the network
        // Like a pre-screen that waits for user input. After user presses something the attempt to connect to network occurs
		SessionManager.Instance.StartSession();
	}

	public void CreateGame_Click()
	{
		SessionManager.Instance.CreateRoom();
        PlayShortClick();
	}

	public void JoinGame_Click()
	{
        PlayShortClick();
	}

	private void ConnectedToNetwork()
	{
		// Perform actions necessary to tell user they are successfully connected
	}

	private void DisconnectedFromNetwork(DisconnectCause cause)
	{
		// Perform actoins necessary to tell the user they have been disconnected
        ShowRoomInfo = false;
	}

	private void RoomCreated()
	{
        PlayShortClick();
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
