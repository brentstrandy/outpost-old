using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GUIMenuManager : MonoBehaviour 
{
	public bool ShowDebugLogs = true;
    public List<GameObject> Canvases, Panels, MenuHeaders, Buttons;
    [HideInInspector]
    public string CurrentMenu, PreviousMenu;
    [HideInInspector]
    public GameObject CurrentMenu_GO, PreviousMenu_GO;
    [HideInInspector]
    public AudioClip ShortClick, LongClick;

    void Start()
    {
        CurrentMenu = "Main Menu";

        #region Lists of GUI Menu elements
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

        #region Sound Effects
        string soundEffectsPath = "Audio/Effects/";
        ShortClick = Resources.Load(soundEffectsPath + "short_click_01") as AudioClip;
        LongClick = Resources.Load(soundEffectsPath + "long_click_01") as AudioClip;
        #endregion
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
    #endregion

    public void PlayShortClick()
    {
        audio.PlayOneShot(ShortClick);
    }

    public void PlayLongClick()
    {
        audio.PlayOneShot(LongClick);
    }

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


    public void CreateGame()
    {
        SessionManager.Instance.CreateRoom();
    }

	/// <summary>
	/// Called when a room is created
	/// </summary>
	private void RoomCreated()
	{
		// Change menu to reflect a room being created
		this.Log("Created Room");
	}

	private void connected()
	{
		Debug.Log ("Button Test: Connected");
	}

	private void disconnected(DisconnectCause cause)
	{
		Debug.Log("DISCONNECT: " + cause.ToString());
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