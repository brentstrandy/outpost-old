using UnityEngine;
using System.Collections;

public class ButtonTest : MonoBehaviour 
{
	public bool ShowDebugLogs = true;

    Canvas can_0;

    public void ButtonPress()
    {
		Debug.Log("Button Press");
    }

    public void ButtonPress(string buttonName)
    {
        Debug.Log(buttonName);
    }

	public void Start()
	{

	}

	public void CreateGame()
	{
		SessionManager.Instance.CreateRoom();
	}

    public void SwitchCamera(string canvasName)
    {
        //Canvas can_all = GameObject.Find("Canvas") as Canvas;
        
        can_0 = GetComponent("Canvas (All)/Canvas0") as Canvas;
        //can_0 = GetComponent<Canvas>();
        //Canvas can_1 = GetComponent(canvasName) as Canvas;

        //if (can_all)
        //{
            

        //    //can_0.enabled = false;
        //    //can_1.enabled = true;
        //}
        //else
        //    Debug.LogError("Cannot find Canvas in scene.");
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