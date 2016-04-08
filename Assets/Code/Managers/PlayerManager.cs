using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player is a Singleton used for the entirety of the game session.
/// It is created when the user starts a game session.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    private static PlayerManager instance;
    public bool ShowDebugLogs = true;

	// List of ALL players currently in the game
	public List<Player> CurrentPlayerList { get; private set; }
	// Reference to the current player (local player)
	public CurrentPlayer CurPlayer { get; private set; }
	// Used to limit how often game checks for returning players
	private float LastTimeChecked;

	// Delegates
	public delegate void PlayerManagerAction();
	public event PlayerManagerAction OnEndGameDataSaved;
	public event PlayerManagerAction OnCurPlayerDataDownloaded;

    // Components
    private PhotonView ObjPhotonView;

    public void Start()
	{
        // The player needs to know when connection has been made to the server so that it can set its data
        SessionManager.Instance.OnSMConnected += Connected_Event;
        SessionManager.Instance.OnSMDisconnected += Disconnected_Event;

		SessionManager.Instance.OnSMPlayerJoinedRoom += OnRemotePlayerJoined_Event;
		SessionManager.Instance.OnSMPlayerLeftRoom += OnRemotePlayerLeft_Event;
		SessionManager.Instance.OnSMJoinedRoom += OnLocalPlayerJoined_Event;
		SessionManager.Instance.OnSMLeftRoom += OnLocalPlayerLeft_Event;

        ObjPhotonView = PhotonView.Get(this);

		LastTimeChecked = Time.time;

		CurrentPlayerList = new List<Player>();
    }

    #region INSTANCE (SINGLETON)

    /// <summary>
    /// Singleton - There can be only one
    /// </summary>
    /// <value>The instance.</value>
    public static PlayerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<PlayerManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    #endregion

    #region EVENTS

    /// <summary>
    /// Called when the game connects to the network. The player is instantiated with the login credentials
    /// </summary>
    private void Connected_Event()
    {
		Log("[Local Player] Connected");
		// For security reasons, userID should only be used by the current (local) player to authenticate their ACCOUNT (private) data
		// All other data - like the LEVEL PROGRESS or PROFILE data can be attained with just the username
		
		// Get the database ID that represents the player
		string userID = PhotonNetwork.AuthValues.UserId;
		// Get the username that represents the player
		string username = SessionManager.Instance.GetPlayerInfo().name;

		CurPlayer = new CurrentPlayer(SessionManager.Instance.GetPlayerInfo());
		CurPlayer.OnPlayerDataDownloaded += OnCurPlayerDataDownloaded_Event;

		Log("Downloading [Local Player] Details from Diadem's server");
        
		WWWForm form = new WWWForm();
		// Load player level progress data - based on the userID (aquired when logging into Diadem's server)
		form.AddField("accountID", userID);
		StartCoroutine(CurPlayer.LevelProgressDataManager.LoadDataFromServer("http://www.diademstudios.com/outpostdata/PlayerData_LevelProgress.php", form));
		// Load player tower progress data - based on the userID (acquired when loggin into Diadem's server)
		StartCoroutine(CurPlayer.TowerProgressDataManager.LoadDataFromServer("http://www.diademstudios.com/outpostdata/PlayerData_TowerProgress.php", form));
		// Load player account details - based on the userID (acquired when loggin into Diadem's server)
		StartCoroutine(CurPlayer.AccountDataManager.LoadDataFromServer("http://www.diademstudios.com/outpostdata/PlayerData_AccountData.php", form));
		// Load player profile details - based on the userID (acquired when loggin into Diadem's server)
		StartCoroutine(CurPlayer.ProfileDataManager.LoadDataFromServer("http://www.diademstudios.com/outpostdata/PlayerData_ProfileData.php", form));
    }

    private void Disconnected_Event()
    {
		// Clear all player lists - we are no longer connected to the game
		CurrentPlayerList.Clear();
		// Keep the CurPlayer as they never leave (until the game is shutdown)
		CurrentPlayerList.Add(CurPlayer);
    }

	private void OnRemotePlayerJoined_Event(PhotonPlayer player)
	{
		string username = player.name;
		int index = CurrentPlayerList.FindIndex(x => x.Username == username);

		Log("Player joined (" + player.name + ")");

		// Check to see if the player joining had previously left in the middle of this game
		if(index == -1)
		{
			// Only add the player if the game isn't currently running (lobby), otherwise boot them out
			if(GameManager.Instance == null)
				AddPlayerToCurrentList(player);
			else
			{
				// Only the session manager should boot a player
				if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
					SessionManager.Instance.KickPlayer(player);
			}
		}
		else
		{
			Log("Player Returned: " + player.name);
			// Reconnect the player who had previously disconnected and assign the player's towers back to them
			CurrentPlayerList[index].ReconnectPlayer(player);
			GameManager.Instance.TowerManager.ReassignPlayerTowers(player.name, player);

			// Have Master Client Raise an Event to give the joining player information about the game
			if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
			{
				// Send the rejoining player a message telling them that they can join
				SessionManager.Instance.RaiseEvent((byte)RaiseEventCode.AllowPlayerToJoin, true, true, new RaiseEventOptions { TargetActors = new int[] { player.ID } } );
			}
		}
	}

	private void OnRemotePlayerLeft_Event(PhotonPlayer player)
	{
		string username = player.name;
		int index = CurrentPlayerList.FindIndex(x => x.Username == username);

		// Only take action if the player can be found in the currnet list of players
		if(index != -1)
		{
			// When in-game, the player has time to return and reclaim their towers. When in the lobby, this doesn't happen
			if(GameManager.Instance != null && GameManager.Instance.GameRunning)
			{	
				Log("Player left (" + username + "). Freezing towers for a short period to allow player to return.");
				// Set the player's current connection status as disconnected
				CurrentPlayerList[CurrentPlayerList.FindIndex(x => x.Username == username)].DisconnectPlayer();

				// Tell the Tower Manager to "freeze" all of the player's towers for 5 minutes
				GameManager.Instance.TowerManager.FreezePlayerTowers(player.name);
			}
			else
			{
				// Game is currently in the lobby. We can simply remove the player
				CurrentPlayerList.RemoveAt(index);
			}
		}
		else
			Log("Player left, but we don't know who it was :(");
	}

	private void OnLocalPlayerJoined_Event()
	{
		// Run through all current players in the room and add them to the player's list of current players
		foreach(PhotonPlayer p in SessionManager.Instance.GetOtherPlayersInRoom())
		{
			AddPlayerToCurrentList(p);
		}
	}

	private void OnLocalPlayerLeft_Event()
	{
		// Clear all player lists - we are no longer connected to the room
		CurrentPlayerList.Clear();
		// Keep the CurPlayer as they never leave (until the game is shutdown)
		CurrentPlayerList.Add(CurPlayer);
	}

	private void OnCurPlayerDataDownloaded_Event()
	{
		if(OnCurPlayerDataDownloaded != null)
			OnCurPlayerDataDownloaded();
	}

    #endregion

	/// <summary>
	/// Starts a coroutine that saves game data to the server
	/// </summary>
	/// <param name="waitForResponse">If set to <c>true</c> will fire an event to proclaim the save complete.</param>
	public void SavePlayerGameDataToServer(bool waitForResponse = true)
	{
		// Save player data to server
		StartCoroutine(SaveDataToServer(waitForResponse));
	}

	private void AddPlayerToCurrentList(PhotonPlayer photonPlayer)
	{
		Player p = new Player(photonPlayer);

		Log("Player Joined. Downloading profile data from server.");
		// Load player profile details
		WWWForm form = new WWWForm();
		form.AddField("accountID", photonPlayer.name);
		StartCoroutine(p.ProfileDataManager.LoadDataFromServer("http://www.diademstudios.com/outpostdata/PlayerData_ProfileData.php", form));

		// Save player to a list of all current players
		CurrentPlayerList.Add(p);
	}

    public void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.GameRunning)
        {
			// Only check status of removed players every THREE seconds
			if(Time.time - LastTimeChecked > 3)
			{
				// Loop through every player that has dropped to see when it is safe for them to be permanently removed from the game
				foreach(Player p in CurrentPlayerList.FindAll(x => x.Connected == false))
				{
					Log("Testing " + p.Username + " to see if they have returned. Total Disconnect Duration: " + p.DisconnectedDuration());
					// If player is gone for a certain amount of time, remove them from the game
					if(p.DisconnectedDuration() > 5)
					{
						Log(p.Username + " ran out of time. Towers being reallocated");
						// Reassign player's towers to the current Master Client
						GameManager.Instance.TowerManager.ReassignPlayerTowers(p.Username, SessionManager.Instance.GetMasterClient());
					}
				}

				// Remove all players from the CurrentPlayerList who have been disconnected for too long
				// ** DisconnectedDuration is slightly longer in case a player leaves in the middle of this algorithm
				CurrentPlayerList.RemoveAll(x => x.DisconnectedDuration() > 5.1f);

				LastTimeChecked = Time.time;
			}
        }
    }

    public void InformPlayerOfDamagedEnemy(PhotonPlayer player, string enemyName, float thraceiumDamage, float ballisticDamage, bool kill)
    {
        ObjPhotonView.RPC("EnemyDamaged", player, enemyName, thraceiumDamage, ballisticDamage, kill);
    }

    [PunRPC]
    public void EnemyDamaged(string enemyName, float thraceiumDamage, float ballisticDamage, bool kill)
    {
        // TO DO: Track the amount of thraceium and ballistic damage dealt by the player

        if (kill)
        {
            // Increase player's score
			CurPlayer.ProccessKill(GameDataManager.Instance.FindEnemyDataByDisplayName(enemyName).ScoreValue);
            // Tell the player they made a kill
            NotificationManager.Instance.DisplayNotification(new NotificationData("", "Killed " + enemyName + "  +" + GameDataManager.Instance.FindEnemyDataByDisplayName(enemyName).ScoreValue.ToString(), "QuickInfo"));
        }
    }

	/// <summary>
	/// Sends player's stats to the server for the currently ended game.
	/// </summary>
	/// <returns>Nothing.</returns>
	/// <param name="waitForResponse">If set to <c>true</c> will fire an event to proclaim the save complete.</param>
	private IEnumerator SaveDataToServer(bool waitForResponse = true)
    {
        // Save a local copy of the player's progress so that they can keep playing the game and see the progress
		if(GameManager.Instance.Victory)
			CurPlayer.LevelProgressDataManager.DataList.Add(new PlayerLevelProgressData(GameManager.Instance.CurrentLevelData.LevelID, CurPlayer.Score));

        // Call web service that saves the player's progress
		WWWForm form = new WWWForm();
		form.AddField("accountID", CurPlayer.AccountID.ToString());
		form.AddField("gameID", GameManager.Instance.GameID.ToString());
		form.AddField("levelID", GameManager.Instance.CurrentLevelData.LevelID.ToString());
		form.AddField("score", CurPlayer.Score.ToString());
		form.AddField("kills", CurPlayer.KillCount.ToString());
		form.AddField("victory", GameManager.Instance.Victory.ToString());
		form.AddField("finishedGame", (!GameManager.Instance.GameRunning).ToString());

		WWW www = new WWW("http://www.diademstudios.com/outpostdata/Action_SavePlayerStats.php", form);
	    
		// Some requests need to happen immeidately and cannot wait for a response (quitting the game)
		if(waitForResponse)
		{
			while (!www.isDone)
	        {
	            yield return 0;
	        }

			Log("Player Game Data and Level Progress Saved to Server");

			// Trigger event to tell anyone listening that the data finished saving
			if(OnEndGameDataSaved != null)
				OnEndGameDataSaved();
		}
    }

    public List<TowerData> GetGameLoadOutTowers()
    {
        List<TowerData> towerNames = new List<TowerData>();

        if (CurPlayer.GameLoadOut != null)
            towerNames = CurPlayer.GameLoadOut.Towers;

        return towerNames;
    }

    public void ResetData()
    {
		CurPlayer.ResetData();
		//Destroy(PlayerLocator);
    }

	private void OnDestroy()
	{
		// Remove all references to delegate events that were created for this script
		if(SessionManager.Instance != null)
		{
			SessionManager.Instance.OnSMConnected -= Connected_Event;
			SessionManager.Instance.OnSMDisconnected -= Disconnected_Event;

			SessionManager.Instance.OnSMPlayerJoinedRoom -= OnRemotePlayerJoined_Event;
			SessionManager.Instance.OnSMPlayerLeftRoom -= OnRemotePlayerLeft_Event;
			SessionManager.Instance.OnSMJoinedRoom -= OnLocalPlayerJoined_Event;
			SessionManager.Instance.OnSMLeftRoom -= OnLocalPlayerLeft_Event;
		}
	}

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[PlayerManager] " + message);
    }

    protected void LogError(string message)
    {
        Debug.LogError("[PlayerManager] " + message);
    }

    #endregion MessageHandling
}