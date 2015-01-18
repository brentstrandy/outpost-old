using UnityEngine;
using System.Collections;

public class SessionManager : MonoBehaviour
{
	/// <summary>
	/// Define whether to show debug messages in the console window or hide them
	/// </summary>
	public bool ShowDebugLogs = true;

	public bool ConnectedToPhoton { get; private set; }

	// Use this for initialization
	void Start ()
	{
		PhotonNetwork.ConnectUsingSettings("v1.0");
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	#region PHOTON CONNECT/DISCONNECT
	/// <summary>
	/// Called when the initial connection got established but before you can use the server.
	/// </summary>
	public void OnConnectedToPhoton()
	{
		ConnectedToPhoton = true;
		this.Log("PHOTON: Connected");
	}

	/// <summary>
	/// Called if a connect call to the Photon server failed before the connection was established, followed by a call to OnDisconnectedFromPhoton().
	/// </summary>
	/// <param name="cause">Cause for failure.</param>
	public void OnFailedToConnectToPhoton(DisconnectCause cause)
	{
		ConnectedToPhoton = false;
		this.LogError("PHOTON: Failed to connect - connection not established");
	}

	/// <summary>
	/// Called when something causes the connection to fail (after it was established), followed by a call to OnDisconnectedFromPhoton().
	/// </summary>
	/// <param name="cause">Cause for failure.</param>
	public void OnConnectionFail(DisconnectCause cause)
	{
		ConnectedToPhoton = false;
		this.LogError("PHOTON: Failed to connect after establishing connection");
	}

	/// <summary>
	/// Called after disconnecting from the Photon server.
	/// In some cases, other callbacks are called before OnDisconnectedFromPhoton is called.
	/// </summary>
	/// <param name="cause">Cause for failure.</param>
	public void OnDisconnectedFromPhoton()
	{
		ConnectedToPhoton = false;
		this.Log("PHOTON: Disconnected from Photon");
	}

	/// <summary>
	/// Called when the custom authentication failed. Followed by disconnect! (Only applicable to custom authentication services)
	/// </summary>
	/// <param name="debugMessage">Debug message.</param>
	public void OnCustomAuthenticationFailed(string debugMessage)
	{
		ConnectedToPhoton = false;
		this.LogError("PHOTON: " + debugMessage);
	}
	#endregion

	#region PHOTON LOBBY
	/// <summary>
	/// Called on entering the Master Server's lobby. The actual room-list updates will call OnReceivedRoomListUpdate()
	/// Note: When PhotonNetwork.autoJoinLobby is false, OnConnectedToMaster() will be called and the room list won't become available.
	/// </summary>
	public void OnJoinedLobby()
	{
		this.Log("PHOTON: Connected to Master Server's lobby");
	}

	/// <summary>
	/// Called after the connection to the master is established and authenticated but only when PhotonNetwork.autoJoinLobby is false
	/// </summary>
	public void OnConnectedToMaster()
	{
		this.Log("PHOTON: Connected to Master - no rooms available unless connecting to a lobby");
	}

	/// <summary>
	/// Called after leaving a lobby
	/// </summary>
	public void OnLeftLobby()
	{
	}
	#endregion

	#region PHOTON ROOM
	/// <summary>
	/// Called once the local user left a room.
	/// </summary>
	public void OnLeftRoom()
	{
		this.Log("PHOTON: Left Room");
	}

	/// <summary>
	/// Called when a CreateRoom() call failed.
	/// </summary>
	/// <param name="codeAndMsg">codeAndMsg[0] is int ErrorCode. codeAndMsg[1] is string debug msg.</param>
	public void OnPhotonCreateRoomFailed(object[] codeAndMsg)
	{
		this.LogError("PHOTON: (" + codeAndMsg[0] + ") " + codeAndMsg[1]);
	}

	/// <summary>
	/// Called when a JoinRoom() call failed.
	/// </summary>
	/// <param name="codeAndMsg[0] is int ErrorCode. codeAndMsg[1] is string debug msg.</param>
	public void OnPhotonJoinRoomFailed(object[] codeAndMsg)
	{
		this.LogError("PHOTON: (" + codeAndMsg[0] + ") " + codeAndMsg[1]);
	}

	/// <summary>
	/// Called when this client created a room and is in it. OnJoinedRoom() will be called next as you entered a room.
	/// This callback is only called on the client which created a room.
	/// </summary>
	public void OnCreatedRoom()
	{
		this.Log("PHOTON: Created Room");
	}

	/// <summary>
	/// Called when entering a room (by creating or joining it). Called on all clients (including the Master Client).
	/// </summary>
	public void OnJoinedRoom()
	{
		//PhotonNetwork.playerList  Room.customProperties
		this.Log("PHOTON: Joined Room");
	}

	/// <summary>
	/// Called after switching to a new MasterClient when the current one leaves. The former already got removed from the player list.
	/// </summary>
	/// <param name="newMasterClient">New master client.</param>
	public void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		this.Log("PHOTON: MasterClient left. Switched Master Client to " + newMasterClient.name);
	}
	#endregion

	#region PHOTON PLAYER
	/// <summary>
	/// Called when a remote player entered the room. This PhotonPlayer is already added to the playerlist at this time.
	/// </summary>
	/// <param name="newPlayer">New player.</param>
	public void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		// TODO: Check player count to see if game can start
		//Room.playerCount
		
		this.Log("PHOTON: " + player.name + " joined the room");
	}
	
	/// <summary>
	/// Called when a remote player left the room. This PhotonPlayer is already removed from the playerlist at this time.
	/// </summary>
	/// <param name="newPlayer">Disconnected player.</param>
	public void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		this.Log("PHOTON: " + player.name + " left the room");
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
