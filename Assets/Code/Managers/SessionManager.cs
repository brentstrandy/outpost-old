using UnityEngine;
using ExitGames.Client.Photon;

/// <summary>
/// Manages Network Session - Singleton
/// Owner: Brent Strandy
/// </summary>
public class SessionManager : MonoBehaviour
{
    private static SessionManager instance;
    public bool ShowDebugLogs = true;
    public bool Offline { get; private set; }

    #region EVENTS (DELEGATES)

    public delegate void SessionManagerAction();

    public delegate void SessionManagerActionFailure(DisconnectCause cause);

    public delegate void SessionManagerActionRoomFailure(object[] codeAndMsg);

	public delegate void SessionManagerActionCustPlayerProperties(object[] parameters);

	public delegate void SessionManagerActionCustRoomProperties(Hashtable propertiesThatChanged);

    public delegate void SessionManagerActionPlayer(PhotonPlayer player);

    public delegate void SessionManagerActionAuthFailed(string message);

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
    /// Called when the list of rooms in a lobby is updated
    /// </summary>
    public event SessionManagerAction OnSMReceivedRoomListUpdate;

    /// <summary>
    /// Called when player leaves a room
    /// </summa
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
    /// Called when the player's login authentication failed
    /// </summary>
    public event SessionManagerActionAuthFailed onSMAuthenticationFail;

    /// <summary>
    /// Called when the room fails to create
    /// </summary>
    public event SessionManagerActionRoomFailure OnSMCreateRoomFail;

    /// <summary>
    /// Called when the player failed to join the room
    /// </summary>
    public event SessionManagerActionRoomFailure OnSMJoinRoomFail;

    /// <summary>
    /// Called when the player failed to randomly join a room
    /// </summary>
    public event SessionManagerActionRoomFailure OnSMRandomJoinFail;

    /// <summary>
    /// Called when the master client was switched to a different client
    /// </summary>
    public event SessionManagerActionPlayer OnSMSwitchMaster;

	/// <summary>
	/// Called when the custom properties for the room have changed
	/// </summary>
	public event SessionManagerActionCustRoomProperties OnSMRoomPropertiesChanged;

	/// <summary>
	/// Called when the custom properties for any player have changed
	/// </summary>
	public event SessionManagerActionCustPlayerProperties OnSMPlayerPropertiesChanged;

    #endregion EVENTS (DELEGATES)

    #region INSTANCE (SINGLETON)

    /// <summary>
    /// Singleton - There can only be one
    /// </summary>
    /// <value>The instance.</value>
    public static SessionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<SessionManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    #endregion INSTANCE (SINGLETON)

    #region ACTIONS

    /// <summary>
    /// Start the network session - only if one is not currently started
    /// </summary>
    public void StartSession(bool autoJoinLobby = true)
    {
        PhotonNetwork.autoJoinLobby = autoJoinLobby;
        // By default, do not clean up GameObjects when a players disconnects
        PhotonNetwork.autoCleanUpPlayerObjects = false;

        // Only connect if the player is not already connected
        if (!PhotonNetwork.connected)
            PhotonNetwork.ConnectUsingSettings("v1.0");

		PhotonNetwork.OnEventCall += OnEvent;
    }

    /// <summary>
    /// End the current network session - only if one is currently open
    /// </summary>
    public void EndSession()
    {
        // Only disconnect if the player is already connected
        if (PhotonNetwork.connected)
            PhotonNetwork.Disconnect();
    }

    public void AuthenticatePlayer(string name, string password)
    {
        PhotonNetwork.AuthValues = new AuthenticationValues();
        PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Custom;
        PhotonNetwork.AuthValues.AddAuthParameter("username", name);
        PhotonNetwork.AuthValues.AddAuthParameter("password", password);
    }

	public void RaiseEvent(byte eventCode, object eventContent, bool sendReliable, RaiseEventOptions options)
	{
		PhotonNetwork.RaiseEvent(eventCode, eventContent, sendReliable, options);
	}

    /// <summary>
    /// Joins the default lobby (master server lobby)
    /// </summary>
    public void JoinLobby()
    {
        if (!PhotonNetwork.insideLobby)
            PhotonNetwork.JoinLobby();
    }

    /// <summary>
    /// Leaves the player's current lobby
    /// </summary>
    public void LeaveLobby()
    {
        if (PhotonNetwork.insideLobby)
            PhotonNetwork.LeaveLobby();
    }

    /// <summary>
    /// Sets the offline mode.
    /// </summary>
    /// <param name="offline">If set to <c>true</c>, offline.</param>
    public void SetOfflineMode(bool offline)
    {
        PhotonNetwork.offlineMode = offline;
    }

    /// <summary>
    /// Creates and joins a new room
    /// </summary>
    public void CreateRoom()
    {
        // Only attempt to create a room if you are not currently in a room
        if (!PhotonNetwork.inRoom)
            PhotonNetwork.CreateRoom("");
    }

    /// <summary>
    /// Creates and joins a new room with predefined options
    /// </summary>
    /// <param name="name">Name - empty string forces server to create random name</param>
    /// <param name="roomOptions">Room options.</param>
    /// <param name="typedLobby">Default or SQL - SQL allows for complex 'where' clauses.</param>
    public void CreateRoom(string name, RoomOptions roomOptions, TypedLobby typedLobby)
    {
		if(roomOptions == null)
			roomOptions = new RoomOptions() { isVisible = true, isOpen = true, maxPlayers = 8 };
		
        if (!PhotonNetwork.inRoom)
            PhotonNetwork.CreateRoom(name, roomOptions, typedLobby);
    }

    /// <summary>
    /// Joins a room
    /// </summary>
    /// <param name="roomName">Room name.</param>
    public void JoinRoom(string roomName)
    {
        // Only attempt to create a room if you are not currently in a room
        if (!PhotonNetwork.inRoom)
            PhotonNetwork.JoinRoom(roomName);
    }

    /// <summary>
    /// Attempts to join a random room
    /// </summary>
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// Leaves the current room - only if the player is in one
    /// </summary>
    public void LeaveRoom()
    {
        // Only attempt to leave a room if the player is in a room
        if (PhotonNetwork.inRoom)
            PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// Sets the visibility of the current room
    /// </summary>
    /// <param name="newValue">If set to <c>true</c>, room can be seen by everyone. <c>false</c> means by invite only</param>
    public void SetRoomVisibility(bool newValue)
    {
        if (PhotonNetwork.inRoom)
            PhotonNetwork.room.visible = newValue;
    }

    /// <summary>
    /// Sets the room to either open or closed. When closed, no one can join
    /// </summary>
    /// <param name="newValue">If set to <c>true</c> new value.</param>
    public void SetRoomOpen(bool newValue)
    {
        if (PhotonNetwork.inRoom)
            PhotonNetwork.room.open = newValue;
    }

	/// <summary>
	/// Sets the room custom properties.
	/// </summary>
	/// <param name="properties">Properties to set</param>
	public void SetRoomCustomProperties(Hashtable properties)
	{
		PhotonNetwork.room.SetCustomProperties(properties);
	}

    /// <summary>
    /// Sets custom properties of the player as defined in the Hashtable.
    /// </summary>
    /// <param name="properties">Properties to set</param>
    public void SetPlayerCustomProperties(Hashtable properties)
    {
        PhotonNetwork.player.SetCustomProperties(properties);
    }

    /// <summary>
    /// Instantiate an object and propogate the object across the network
    /// </summary>
    /// <param name="name">Prefab name - must be found in \Resources.</param>
    /// <param name="position">Starting Position of Game Object.</param>
    /// <param name="rotation">Starting Rotation (usually Quaterion.identity)</param>
    public GameObject InstantiateObject(string name, Vector3 position, Quaternion rotation)
    {
        return PhotonNetwork.Instantiate(name, position, rotation, 0);
    }

    /// <summary>
    /// Destroy an object and tell all clients to destroy the game object
    /// </summary>
    /// <param name="obj">Object to be destroyed</param>
    public void DestroyObject(GameObject obj)
    {
        PhotonNetwork.Destroy(obj);
    }

    /// <summary>
    /// Closes the connection of a player - KICK/BOOT player from the room.
    /// </summary>
    /// <param name="player">Player.</param>
    public void KickPlayer(PhotonPlayer player)
	{
		Log("Kicked Player: " + player.name);
        PhotonNetwork.CloseConnection(player);
    }

    /// <summary>
    /// Disconnect the player from Photon
    /// </summary>
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    #endregion ACTIONS

    #region INFORMATION

	public int AllocateNewViewID()
	{
		return PhotonNetwork.AllocateViewID();
	}

    /// <summary>
    /// Returns if the player is currently in a room
    /// </summary>
    /// <returns><c>true</c>, if in room, <c>false</c> otherwise.</returns>
    public bool InRoom()
    {
        return PhotonNetwork.inRoom;
    }

    /// <summary>
    /// Returns if the room has reached the maximum number of players
    /// </summary>
    /// <returns><c>true</c> if the current room filled; otherwise, <c>false</c>.</returns>
    public bool IsRoomFilled()
    {
        return (PhotonNetwork.room.maxPlayers == PhotonNetwork.room.playerCount);
    }

    /// <summary>
    /// Gets the current players information
    /// </summary>
    /// <returns>The player info.</returns>
    public PhotonPlayer GetPlayerInfo()
    {
        return PhotonNetwork.player;
    }

    /// <summary>
    /// Gets the current room's max player size
    /// </summary>
    /// <returns>The max room size.</returns>
    public int GetMaxRoomSize()
    {
        return PhotonNetwork.room.maxPlayers;
    }

    /// <summary>
    /// Gets the number of players currently in the room
    /// </summary>
    /// <returns>The room player count.</returns>
    public int GetRoomPlayerCount()
    {
        return PhotonNetwork.room.playerCount;
    }

    /// <summary>
    /// Gets the list of rooms in the current lobby
    /// </summary>
    /// <returns>The room list</returns>
    public RoomInfo[] GetRoomList()
    {
        return PhotonNetwork.GetRoomList();
    }

    /// <summary>
    /// Gets the room information of the current room
    /// </summary>
    /// <returns>The current room info.</returns>
    public Room GetCurrentRoomInfo()
    {
        return PhotonNetwork.room;
    }

    /// <summary>
    /// Gets a list of all players in the room, including the current player
    /// </summary>
    /// <returns>The all players in room.</returns>
    public PhotonPlayer[] GetAllPlayersInRoom()
    {
        return PhotonNetwork.playerList;
    }

    /// <summary>
    /// Gets a list of all other players in the room - this does not include the current player
    /// </summary>
    /// <returns>The other players in room.</returns>
    public PhotonPlayer[] GetOtherPlayersInRoom()
    {
        return PhotonNetwork.otherPlayers;
    }

    /// <summary>
    /// Gets all the current custom properties applied to the player
    /// </summary>
    /// <returns>The player's custom properties</returns>
    public ExitGames.Client.Photon.Hashtable GetPlayerCustomProperties()
    {
        return PhotonNetwork.player.customProperties;
    }


	/// <summary>
	/// Gets all the current custom properties applied to the room
	/// </summary>
	/// <returns>The room's custom properties</returns>
	public ExitGames.Client.Photon.Hashtable GetRoomCustomProperties()
	{
		return PhotonNetwork.room.customProperties;
	}

    public void LoadGameLevel(string level)
    {
        PhotonNetwork.LoadLevel(level);
    }

	/// <summary>
	/// Gets the master client PhotonPlayer
	/// </summary>
	/// <returns>The master client.</returns>
	public PhotonPlayer GetMasterClient()
	{
		return PhotonNetwork.masterClient;
	}

    public int GetMasterClientID()
    {
        return PhotonNetwork.masterClient.ID;
    }

    #endregion INFORMATION

    #region PHOTON CONNECT/DISCONNECT

    /// <summary>
    /// Called when the initial connection got established but before you can use the server.
    /// Technically nothing should happen at this point in time because network services are not available
    /// </summary>
    private void OnConnectedToPhoton()
    {
        this.Log("Connected");

        // Call delegate event linked to this action
        if (OnSMConnected != null)
            OnSMConnected();
    }

    /// <summary>
    /// Called after the connection to the master is established and authenticated but only when PhotonNetwork.autoJoinLobby is false
    /// </summary>
    private void OnConnectedToMaster()
    {
        this.Log("Connected to Master - no rooms available unless connecting to a lobby");

        // Call delegate event linked to this action
        if (OnSMConnected != null)
            OnSMConnected();
    }

    /// <summary>
    /// Called if a connect call to the Photon server failed before the connection was established, followed by a call to OnDisconnectedFromPhoton().
    /// </summary>
    /// <param name="cause">Cause for failure.</param>
    private void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        this.LogError("Failed to connect - connection not established");

        // Call delegate event linked to this action
        if (OnSMConnectionFail != null)
            OnSMConnectionFail(cause);
    }

    /// <summary>
    /// Called when something causes the connection to fail (after it was established), followed by a call to OnDisconnectedFromPhoton().
    /// </summary>
    /// <param name="cause">Cause for failure.</param>
    private void OnConnectionFail(DisconnectCause cause)
    {
        this.LogError("Failed to connect after establishing connection");

        // Call delegate event linked to this action
        if (OnSMConnectionFail != null)
            OnSMConnectionFail(cause);
    }

    /// <summary>
    /// Called after disconnecting from the Photon server.
    /// In some cases, other callbacks are called before OnDisconnectedFromPhoton is called.
    /// </summary>
    /// <param name="cause">Cause for failure.</param>
    private void OnDisconnectedFromPhoton()
    {
        this.Log("Disconnected from Photon");

        // Call delegate event linked to this action
        if (OnSMDisconnected != null)
            OnSMDisconnected();
    }

    /// <summary>
    /// Called when the custom authentication failed. Followed by disconnect! (Only applicable to custom authentication services)
    /// </summary>
    /// <param name="debugMessage">Debug message.</param>
    private void OnCustomAuthenticationFailed(string debugMessage)
    {
        this.Log("Custom authentication failed: " + debugMessage);

        // Call delegate event linked to this action
        if (onSMAuthenticationFail != null)
            onSMAuthenticationFail(debugMessage);
    }

    #endregion PHOTON CONNECT/DISCONNECT

    #region PHOTON LOBBY

    /// <summary>
    /// Called on entering the Master Server's lobby. The actual room-list updates will call OnReceivedRoomListUpdate()
    /// Note: When PhotonNetwork.autoJoinLobby is false, OnConnectedToMaster() will be called and the room list won't become available.
    /// </summary>
    private void OnJoinedLobby()
    {
        this.Log("Connected to Master Server's lobby");

        // Call delegate event linked to this action
        if (OnSMJoinedLobby != null)
            OnSMJoinedLobby();
    }

    /// <summary>
    /// Called after leaving a lobby
    /// </summary>
    private void OnLeftLobby()
    {
        this.Log("Left Lobby");

        // Call delegate event linked to this action
        if (OnSMLeftLobby != null)
            OnSMLeftLobby();
    }

    #endregion PHOTON LOBBY

    #region PHOTON ROOM

    /// <summary>
    /// Called once the local user left a room.
    /// </summary>
    private void OnLeftRoom()
    {
        this.Log("Left Room");

        // Call delegate event linked to this action
        if (OnSMLeftRoom != null)
            OnSMLeftRoom();
    }

    /// <summary>
    /// Called whenever the room list is updated
    /// </summary>
    private void OnReceivedRoomListUpdate()
    {
        this.Log("Received Room list update.");

        // Call delegate event linked to this action
        if (OnSMReceivedRoomListUpdate != null)
            OnSMReceivedRoomListUpdate();
    }

    /// <summary>
    /// Called when a CreateRoom() call failed.
    /// </summary>
    /// <param name="codeAndMsg">codeAndMsg[0] is int ErrorCode. codeAndMsg[1] is string debug msg.</param>
    private void OnPhotonCreateRoomFailed(object[] codeAndMsg)
    {
        this.LogError("Failed to create room (" + codeAndMsg[0] + ") " + codeAndMsg[1]);

        // Call delegate event linked to this action
        if (OnSMCreateRoomFail != null)
            OnSMCreateRoomFail(codeAndMsg);
    }

    private void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        this.Log("Failed to join random room.");

        // Call delegate event linked to this action
        if (OnSMRandomJoinFail != null)
            OnSMRandomJoinFail(codeAndMsg);
    }

    /// <summary>
    /// Called when a JoinRoom() call failed.
    /// </summary>
    /// <param name="codeAndMsg[0] is int ErrorCode. codeAndMsg[1] is string debug msg.</param>
    private void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        this.LogError("Failed to join room (" + codeAndMsg[0] + ") " + codeAndMsg[1]);

        // Call delegate event linked to this action
        if (OnSMJoinRoomFail != null)
            OnSMJoinRoomFail(codeAndMsg);
    }

    /// <summary>
    /// Called when this client created a room and is in it. OnJoinedRoom() will be called next as you entered a room.
    /// This callback is only called on the client which created a room.
    /// </summary>
    private void OnCreatedRoom()
    {
        this.Log("Created Room (" + PhotonNetwork.room.name + ")");

        // Call delegate event linked to this action
        if (OnSMCreatedRoom != null)
            OnSMCreatedRoom();
    }

    /// <summary>
    /// Called when entering a room (by creating or joining it). Called on all clients (including the Master Client).
    /// </summary>
    private void OnJoinedRoom()
    {
        //PhotonNetwork.playerList  Room.customProperties
        this.Log("Joined Room");

        // Call delegate event linked to this action
        if (OnSMJoinedRoom != null)
            OnSMJoinedRoom();
    }

	/// <summary>
	/// Called when any room's custom properties have changed
	/// </summary>
	/// <param name="propertiesThatChanged">Properties that changed.</param>
	private void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
	{
		this.Log("Room (" + PhotonNetwork.room.name + ") properties changed");

		// Call delegate event linked to this action
		if(OnSMRoomPropertiesChanged != null)
			OnSMRoomPropertiesChanged(propertiesThatChanged);
	}

    /// <summary>
    /// Called after switching to a new MasterClient when the current one leaves. The former already got removed from the player list.
    /// </summary>
    /// <param name="newMasterClient">New master client.</param>
    private void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        this.Log("MasterClient left. Switched Master Client to " + newMasterClient.name);

        // Call delegate event linked to this action
        if (OnSMSwitchMaster != null)
            OnSMSwitchMaster(newMasterClient);
    }

    #endregion PHOTON ROOM

	#region PHOTON EVENT

	private void OnEvent(byte eventcode, object content, int senderid)
	{
		switch(eventcode)
		{
		case (byte)RaiseEventCode.AllowPlayerToJoin:
			// Throw event to say that level data was received.

			//PhotonPlayer sender = PhotonPlayer.Find(senderid);  // who sent this?
			//byte[] selected = (byte[])content;
			//foreach (byte unitId in selected)
			//{
				// do something
			//}
			break;
		}
	}

	#endregion

    #region Pv vhgv gggggggggbhcnvfc n  bgf gfjbmjfmnf gfnhHOTON PLAYER

    /// <summary>
    /// Called when a remote player entered the room. This PhotonPlayer is already added to the playerlist at this time.
    /// </summary>
    /// <param name="newPlayer">New player.</param>
    private void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        // TODO: Check player count to see if game can start
        //Room.playerCount

        this.Log("Player (" + player.name + ") joined the room");

        // Call delegate event linked to this action
        if (OnSMPlayerJoinedRoom != null)
            OnSMPlayerJoinedRoom(player);
    }

    /// <summary>
    /// Called when a remote player left the room. This PhotonPlayer is already removed from the playerlist at this time.
    /// </summary>
    /// <param name="newPlayer">Disconnected player.</param>
    private void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        this.Log("Player (" + player.name + ") left the room");

        // Call delegate event linked to this action
        if (OnSMPlayerLeftRoom != null)
            OnSMPlayerLeftRoom(player);
    }

	/// <summary>
	/// Called when any player's custom properties have changed
	/// </summary>
	/// <param name="playerAndUpdatedProps">Player and updated properties.</param>
	private void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
	{
		this.Log("Player (" + ((PhotonPlayer)playerAndUpdatedProps[0]).name + ") properties changed");

		// Call delegate event linked to this action
		if(OnSMPlayerPropertiesChanged != null)
			OnSMPlayerPropertiesChanged(playerAndUpdatedProps);
	}

    #endregion PHOTON PLAYER

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[SessionManager] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[SessionManager] " + message);
    }

    #endregion MessageHandling
}