using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MainMenu_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	
	private void OnEnable()
	{
		// Establish listeners for all applicable events
		SessionManager.Instance.OnSMCreatedRoom += RoomCreated_Event;
		SessionManager.Instance.OnSMCreateRoomFail += CreateRoomFail_Event;
	}
	
	private void OnDisable()
	{
		// Remove listeners for all applicable events
		SessionManager.Instance.OnSMCreatedRoom -= RoomCreated_Event;
		SessionManager.Instance.OnSMCreateRoomFail -= CreateRoomFail_Event;
	}
	
	#region OnClick
	public void CreateGame_Click()
	{
		SessionManager.Instance.CreateRoom("Room of " + PlayerManager.Instance.Username + "(" + System.Guid.NewGuid().ToString() + ")", new RoomOptions() { isVisible = true, isOpen = true, maxPlayers = 8 }, TypedLobby.Default);
	}
	
	public void JoinGame_Click()
	{
		// Tell the MenuManager to transition to the Join Game menu
		MenuManager.Instance.ShowJoinGameMenu();
	}
	
	public void MatchmakingGame_Click()
	{
		// Tell the MenuManager to transition to the MatchMaking menu
		MenuManager.Instance.ShowMatchmakingGameMenu();
	}
	
	public void Settings_Click()
	{
		// Tell the MenuManager to transition to Settings menu
		MenuManager.Instance.ShowSettingsMenu();
	}

	public void Account_Click()
	{
		// Tell the MenuManager to transition to Account menu
		MenuManager.Instance.ShowAccountMenu();
	}

    /// <summary>
    /// Used by the GUI system to go leave the room when the Back button is pressed
    /// </summary>
    public void Back_Click()
    {
        // TO DO: Ask the user if they're sure they want to leave

		// Disconnect the player from Photon
        SessionManager.Instance.Disconnect();

        // Tell the MenuManager to transition back
        MenuManager.Instance.ShowStartMenu();
    }

    #region Used for QuickStart (development mode)
    public void QuickStart_Click()
    {
        #if UNITY_EDITOR
        //LevelData levelData = GameDataManager.Instance.FindLevelDataByDisplayName("Establishing Roots");
        //List<TowerData> availableTowers = {Small Thraceium Tower, EMP Tower};
        //LoadOut towers = new LoadOut();

        //SessionManager.Instance.CreateRoom("Room of " + PlayerManager.Instance.Username + "(" + System.Guid.NewGuid().ToString() + ")", new RoomOptions() { isVisible = true, isOpen = true, maxPlayers = 1 }, TypedLobby.Default);
        
        //PhotonView ObjPhotonView = PhotonView.Get(this);

        ////SessionManager.Instance.SetRoomVisibility(true);

        ////if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        //    //SessionManager.Instance.SetRoomVisibility(false);
        //// Tell all the clients to load the level
        //ObjPhotonView.RPC("LoadLevel", PhotonTargets.All, null);

        //LoadLevel();
        #endif

        LogError("John is lazy. Will get a quick start working later. 9/7/15");
    }

    ///// <summary>
    ///// PunRPC call to tell the client to start loading the level
    ///// </summary>
    //[PunRPC]
    //private void LoadLevel()
    //{
    //    int playerColorIndex = Random.Range(0, 8);
    //    SessionManager.Instance.SetPlayerCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "PlayerColorIndex", playerColorIndex } });

    //    // Record the Loadouts chosen by the player
    //    PlayerManager.Instance.SetGameLoadOut(new LoadOut(TowerLoadoutData));

    //    // Start the game
    //    MenuManager.Instance.ShowStartGame(LevelLoadoutData);
    //}
    #endregion

    #endregion

    #region Events
    private void RoomCreated_Event()
	{
		// Tell the MenuManager to transition to the newly created room
		MenuManager.Instance.ShowRoomDetailsMenu();
	}
	
	private void CreateRoomFail_Event(object[] codeAndMsg)
	{
		this.LogError("Failed to Create Room");
		// TODO: Display an error messages that says room could not be created
		MenuManager.Instance.ShowRoomDetailsMenu();
	}
	#endregion
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[MainMenu_Menu] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[MainMenu_Menu] " + message);
	}
	#endregion
}
