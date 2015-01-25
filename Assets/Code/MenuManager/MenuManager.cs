using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour 
{
	public bool ShowDebugLogs = true;

	private static MenuManager instance;

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

	// Use this for initialization
	void Start ()
	{
		// Track events in order to react to Session Manager events as they happen
		SessionManager.Instance.OnSMConnected += ConnectedToNetwork;
		SessionManager.Instance.OnSMConnectionFail += DisconnectedFromNetwork;
		SessionManager.Instance.OnSMCreatedRoom += RoomCreated;
		SessionManager.Instance.StartSession();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void StartSession_Click()
	{
		// Maybe GUI should have an initial "Start" button. When pressed, the game attampts to connect to the network
		SessionManager.Instance.StartSession();
	}

	public void CreateGame_Click()
	{
		SessionManager.Instance.CreateRoom();
	}

	public void JoinGame_Click()
	{
		
	}

	private void ConnectedToNetwork()
	{
		// Perform actions necessary to tell user they are successfully connected
	}

	private void DisconnectedFromNetwork(DisconnectCause cause)
	{
		// Perform actoins necessary to tell the user they have been disconnected
	}

	private void RoomCreated()
	{
		// Make GUI show connection to room
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
