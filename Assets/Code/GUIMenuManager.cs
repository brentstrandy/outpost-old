using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GUIMenuManager : MonoBehaviour 
{
	public bool ShowDebugLogs = true;

    List<GameObject> Canvases;
    List<GameObject> Panels;
    List<GameObject> MenuHeaders;
    List<GameObject> Buttons;

    string CurrentMenu;
    string PreviousMenu;

    public AudioClip AudioClick0, AudioClick1;

    void Start()
    {
        CurrentMenu = "Main Menu Panel";

        #region Lists of GUI Menu elements
        Canvases = new List<GameObject>(GameObject.FindGameObjectsWithTag("Canvas"));
        Panels = new List<GameObject>(GameObject.FindGameObjectsWithTag("Panel"));
        MenuHeaders = new List<GameObject>(GameObject.FindGameObjectsWithTag("Menu Header"));
        Buttons = new List<GameObject>(GameObject.FindGameObjectsWithTag("Button"));

        if (ShowDebugLogs)
        {
            foreach (GameObject canvas in Canvases)
                Debug.Log("  [CANVAS]" + canvas.name);
            foreach (GameObject panel in Panels)
                Debug.Log("  [PANEL] " + panel.name);
            foreach (GameObject text in MenuHeaders)
                Debug.Log("  [MENU HEADER] " + text.name);
            foreach (GameObject button in Buttons)
                Debug.Log("  [BUTTON] " + button.name);
        }
        #endregion

        Debug.LogError(Application.dataPath);

        AudioClick0 = Resources.Load("/Sounds/Effects/ui_click_01.wav") as AudioClip;
        AudioClick1 = Resources.Load("/Sounds/Effects/ui_click_02.wav") as AudioClip;
    }

    void Update()
    {
        //// make main menu reappear
        //if (Input.GetKeyDown("space"))
        //{
        //    foreach (GameObject button in Buttons)
        //    {
        //        if (button.name == "Create Game")
        //        {
        //            button.transform.parent.gameObject.SetActive(true);
        //        }
        //    }
        //}
    }

    public void ButtonPress()
    {
		Debug.Log("Button Press");
    }

    public void ButtonPress(string buttonName)
    {
        Debug.Log(buttonName);
    }

	public void CreateGame()
	{
		SessionManager.Instance.CreateRoom();
	}

    public void SwitchMenu(string name)
    {
        if (true)
        {
            ButtonSound(AudioClick0);
        }
        //foreach (GameObject button in Buttons)
        //{
        //    if (button.name == name)
        //    {
        //        button.transform.parent.gameObject.SetActive(false);
        //        //button.SetActive(false);
        //        return;
        //    }
        //}

    }

    public void ButtonSound(AudioClip type)
    {
        Debug.Log(type.name);
        if (type)
        {
            Debug.Log("  *BOOP NOISE*");
            audio.PlayOneShot(type);
        }
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