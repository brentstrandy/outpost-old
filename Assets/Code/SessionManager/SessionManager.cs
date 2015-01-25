using UnityEngine;
using System.Collections;

/// <summary>
/// Manages Network Session - Singleton
/// Owner: Brent Strandys
/// </summary>
public class SessionManager : MonoBehaviour
{
	private static SessionManager instance;
	private RoomInfo[] RoomList;

	/// <summary>
	/// Define whether to show debug messages in the console window or hide them
	/// </summary>
	public bool ShowDebugLogs = true;

	/// <summary>
	/// Gets the session I.
	/// </summary>
	/// <value>The session I.</value>
	public int SessionID { get; private set; }
	
	public delegate void SessionManagerAction();
	public delegate void SessionManagerActionFailure(DisconnectCause cause);
	public delegate void SessionManagerActionRoomFailure(object[] codeAndMsg);
	public delegate void SessionManagerActionPlayer(PhotonPlayer player);
	/// <summary>
	/// Called when authenticated (if applicable) and connected to network services
	/// </summary>
	public event SessionManagerAction OnSMConnected;
	/// <summary>
	/// Called when disconnected from network services
	/// </summary>
	public event SessionManagerAction OnSMDisconnected;
	/// <summary>
	/// Called when player successfully joins a lobby
	/// </summary>
	public event SessionManagerAction OnSMJoinedLobby;
	/// <summary>
	/// Called when player leaves a lobby
	/// </summary>
	public event SessionManagerAction OnSMLeftLobby;
	/// <summary>
	/// Called when room is successfully created
	/// </summary>
	public event SessionManagerAction OnSMCreatedRoom;
	/// <summary>
	/// Called when player successfully joins a room
	/// </summary>
	public event SessionManagerAction OnSMJoinedRoom;
	/// <summary>
	/// Called when player leaves a room
	/// </summary>
	public event SessionManagerAction OnSMLeftRoom;
	/// <summary>
	/// Called when another player joins the current room
	/// </summary>
	public event SessionManagerActionPlayer OnSMPlayerJoinedRoom;
	/// <summary>
	/// Called when another player leaves the current room
	/// </summary>
	public event SessionManagerActionPlayer OnSMPlayerLeftRoom;
	/// <summary>
	/// Called when connection to the network services fails
	/// </summary>
	public event SessionManagerActionFailure OnSMConnectionFail;
	/// <summary>
	/// Called when the room fails to create
	/// </summary>
	public event SessionManagerActionRoomFailure OnSMCreateRoomFail;
	/// <summary>
	/// called when the player failed to join the room
	/// </summary>
	public event SessionManagerActionRoomFailure OnSMJoinRoomFail;
	/// <summary>
	/// Called when the master client was switched to a different client
	/// </summary>
	public event SessionManagerActionPlayer OnSMSwitchMaster;

	/// <summary>
	/// Persistent Singleton - this object lasts between scenes
	/// </summary>
	/// <value>The instance.</value>
	public static SessionManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<SessionManager>();
				
				//Tell unity not to destroy this object when loading a new scene!
				DontDestroyOnLoad(instance.gameObject);
			}
			
			return instance;
		}
	}

	void Awake()
	{
		if(instance == null)
		{
			//If I am the first instance, make me the Singleton
			instance = this;
			DontDestroyOnLoad(this);
		}
		else
		{
			//If a Singleton already exists and you find
			//another reference in scene, destroy it!
			if(this != instance)
				Destroy(this.gameObject);
		}
	}

	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void StartSession()
	{
		// Only connect if the player is not already connected
		if(!PhotonNetwork.connected)
			PhotonNetwork.ConnectUsingSettings("v1.0");
	}

	public void EndSession()
	{
		// Only disconnect if the player is already connected
		if(PhotonNetwork.connected)
			PhotonNetwork.Disconnect();
	}

	public void CreateRoom()
	{
		// Only attempt to create a room if you are not currently in a room
		if(!PhotonNetwork.inRoom)
			PhotonNetwork.CreateRoom("");
	}

	public void JoinRoom(string roomName)
	{
		// Only attempt to create a room if you are not currently in a room
		if(!PhotonNetwork.inRoom)
			PhotonNetwork.JoinRoom(roomName);
	}

	public void LeaveRoom()
	{
		// Only attempt to leave a room if the player is in a room
		if(PhotonNetwork.inRoom)
			PhotonNetwork.LeaveRoom();
	}

	public RoomInfo[] GetRoomList()
	{
		return RoomList;
	}

	public Room GetCurrentRoomInfo()
	{
		return PhotonNetwork.room;
	}

	#region PHOTON CONNECT/DISCONNECT
	/// <summary>
	/// Called when the initial connection got established but before you can use the server.
	/// </summary>
	private void OnConnectedToPhoton()
	{
		this.Log("PHOTON: Connected");

		// This connection should only be called if Photon does not plan to autoconnect to a master lobby. If autojoin is enabled
		// then Photon will call OnConnectedToMaster when connection is fully established
		/*if(PhotonNetwork.autoJoinLobby)
		{
			// Call delegate event linked to this action 
			if(OnSMConnected != null)
				OnSMConnected();
		}*/
	}

	/// <summary>
	/// Called after the connection to the master is established and authenticated but only when PhotonNetwork.autoJoinLobby is false
	/// </summary>
	private void OnConnectedToMaster()
	{
		this.Log("PHOTON: Connected to Master - no rooms available unless connecting to a lobby");

		// Call delegate event linked to this action 
		if(OnSMConnected != null)
			OnSMConnected();
	}

	/// <summary>
	/// Called if a connect call to the Photon server failed before the connection was established, followed by a call to OnDisconnectedFromPhoton().
	/// </summary>
	/// <param name="cause">Cause for failure.</param>
	private void OnFailedToConnectToPhoton(DisconnectCause cause)
	{
		this.LogError("PHOTON: Failed to connect - connection not established");

		// Call delegate event linked to this action
		if(OnSMConnectionFail != null)
			OnSMConnectionFail(cause);

	}

	/// <summary>
	/// Called when something causes the connection to fail (after it was established), followed by a call to OnDisconnectedFromPhoton().
	/// </summary>
	/// <param name="cause">Cause for failure.</param>
	private void OnConnectionFail(DisconnectCause cause)
	{
		this.LogError("PHOTON: Failed to connect after establishing connection");

		// Call delegate event linked to this action
		if(OnSMConnectionFail != null)
			OnSMConnectionFail(cause);
	}

	/// <summary>
	/// Called after disconnecting from the Photon server.
	/// In some cases, other callbacks are called before OnDisconnectedFromPhoton is called.
	/// </summary>
	/// <param name="cause">Cause for failure.</param>
	private void OnDisconnectedFromPhoton()
	{
		this.Log("PHOTON: Disconnected from Photon");

		// Call delegate event linked to this action
		if(OnSMDisconnected != null)
			OnSMDisconnected();
	}

	/// <summary>
	/// Called when the custom authentication failed. Followed by disconnect! (Only applicable to custom authentication services)
	/// </summary>
	/// <param name="debugMessage">Debug message.</param>
	private void OnCustomAuthenticationFailed(string debugMessage)
	{
		this.LogError("PHOTON: " + debugMessage);

		// Call delegate event linked to this action
		if(OnSMDisconnected != null)
			OnSMDisconnected();
	}
	#endregion

	#region PHOTON LOBBY
	/// <summary>
	/// Called on entering the Master Server's lobby. The actual room-list updates will call OnReceivedRoomListUpdate()
	/// Note: When PhotonNetwork.autoJoinLobby is false, OnConnectedToMaster() will be called and the room list won't become available.
	/// </summary>
	private void OnJoinedLobby()
	{
		this.Log("PHOTON: Connected to Master Server's lobby");
		RoomList = PhotonNetwork.GetRoomList();

		// Call delegate event linked to this action
		if(OnSMJoinedLobby != null)
			OnSMJoinedLobby();
	}

	/// <summary>
	/// Called after leaving a lobby
	/// </summary>
	private void OnLeftLobby()
	{
		this.Log("PHOTON: Left Lobby");

		// Call delegate event linked to this action
		if(OnSMLeftLobby != null)
			OnSMLeftLobby();
	}
	#endregion

	#region PHOTON ROOM
	/// <summary>
	/// Called once the local user left a room.
	/// </summary>
	private void OnLeftRoom()
	{
		this.Log("PHOTON: Left Room");

		// Call delegate event linked to this action
		if(OnSMLeftRoom != null)
			OnSMLeftRoom();
	}

	/// <summary>
	/// Called when a CreateRoom() call failed.
	/// </summary>
	/// <param name="codeAndMsg">codeAndMsg[0] is int ErrorCode. codeAndMsg[1] is string debug msg.</param>
	private void OnPhotonCreateRoomFailed(object[] codeAndMsg)
	{
		this.LogError("PHOTON: (" + codeAndMsg[0] + ") " + codeAndMsg[1]);

		// Call delegate event linked to this action
		if(OnSMCreateRoomFail != null)
			OnSMCreateRoomFail(codeAndMsg);
	}

	/// <summary>
	/// Called when a JoinRoom() call failed.
	/// </summary>
	/// <param name="codeAndMsg[0] is int ErrorCode. codeAndMsg[1] is string debug msg.</param>
	private void OnPhotonJoinRoomFailed(object[] codeAndMsg)
	{
		this.LogError("PHOTON: (" + codeAndMsg[0] + ") " + codeAndMsg[1]);

		// Call delegate event linked to this action
		if(OnSMJoinRoomFail != null)
			OnSMJoinRoomFail(codeAndMsg);
	}

	/// <summary>
	/// Called when this client created a room and is in it. OnJoinedRoom() will be called next as you entered a room.
	/// This callback is only called on the client which created a room.
	/// </summary>
	private void OnCreatedRoom()
	{
		this.Log("PHOTON: Created Room");

		// Call delegate event linked to this action
		if(OnSMCreatedRoom != null)
			OnSMCreatedRoom();
	}

	/// <summary>
	/// Called when entering a room (by creating or joining it). Called on all clients (including the Master Client).
	/// </summary>
	private void OnJoinedRoom()
	{
		//PhotonNetwork.playerList  Room.customProperties
		this.Log("PHOTON: Joined Room");

		// Call delegate event linked to this action
		if(OnSMJoinedRoom != null)
			OnSMJoinedRoom();
	}

	/// <summary>
	/// Called after switching to a new MasterClient when the current one leaves. The former already got removed from the player list.
	/// </summary>
	/// <param name="newMasterClient">New master client.</param>
	private void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		this.Log("PHOTON: MasterClient left. Switched Master Client to " + newMasterClient.name);

		// Call delegate event linked to this action
		if(OnSMSwitchMaster != null)
			OnSMSwitchMaster(newMasterClient);
	}
	#endregion

	#region PHOTON PLAYER
	/// <summary>
	/// Called when a remote player entered the room. This PhotonPlayer is already added to the playerlist at this time.
	/// </summary>
	/// <param name="newPlayer">New player.</param>
	private void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		// TODO: Check player count to see if game can start
		//Room.playerCount
		
		this.Log("PHOTON: " + player.name + " joined the room");

		// Call delegate event linked to this action
		if(OnSMPlayerJoinedRoom != null)
			OnSMPlayerJoinedRoom(player);
	}
	
	/// <summary>
	/// Called when a remote player left the room. This PhotonPlayer is already removed from the playerlist at this time.
	/// </summary>
	/// <param name="newPlayer">Disconnected player.</param>
	private void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		this.Log("PHOTON: " + player.name + " left the room");

		// Call delegate event linked to this action
		if(OnSMPlayerLeftRoom != null)
			OnSMPlayerLeftRoom(player);
	}
	#endregion

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[SessionManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[SessionManager] " + message);
	}
	#endregion
}
