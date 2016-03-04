using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class LoadLevel_Menu : MonoBehaviour
{
    public bool ShowDebugLogs = true;
    private List<string> PlayerNames;
    private PhotonView ObjPhotonView;

    public GameObject Level_GUIText;

	private int GameID;

    private void Start()
    {
        // Save a handle to the photon view associated with this GameObject for use later
        ObjPhotonView = PhotonView.Get(this);
    }

    private void OnEnable()
    {
		SessionManager.Instance.OnSMRoomPropertiesChanged += RoomCustPropUpdated_Event;

        PlayerNames = new List<string>();

        // TODO: Create a GUI Object for every player, showing their loading status
        foreach (PhotonPlayer player in SessionManager.Instance.GetAllPlayersInRoom())
            PlayerNames.Add(player.name);

        // Show the name and description of the level being loaded
        Level_GUIText.GetComponent<Text>().text = MenuManager.Instance.CurrentLevelData.DisplayName + "\n\n" + MenuManager.Instance.CurrentLevelData.Description;

		// If this is the MasterClient, have them request the server provide a GameID for this game
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			StartCoroutine(GetNewGameID());
		}
    }

    private void OnDisable()
    {
		if(SessionManager.Instance != null)
			SessionManager.Instance.OnSMRoomPropertiesChanged -= RoomCustPropUpdated_Event;

        // TO DO: Remove all GUI Objects previously created
        PlayerNames.Clear();
    }

	private void RoomCustPropUpdated_Event(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
	{
		// Check to see if the level changed
		if(propertiesThatChanged["G_ID"] != null)
		{
			GameID = (int)propertiesThatChanged["G_ID"];

			Log("Acquired GameID: " + propertiesThatChanged["G_ID"].ToString());

			// If the GameManager has already been created, save the GameID to the GameManager so that it is remembered for the entire game
			if(GameManager.Instance != null)
				GameManager.Instance.GameID = GameID;

			// Inform the server that you (the player) is in the game
			AddPlayerToGame(GameID);
		}
	}

    public void Update()
    {
        if (this.enabled)
        {
            // TO DO: Do something interesting for the player to see
        }
    }

    [PunRPC]
    private void StartGame()
    {
        // Tell the GameManager to begin playing the game
        GameManager.Instance.StartGame();

        this.gameObject.SetActive(false);
    }

    [PunRPC]
    private void LevelLoadingComplete(string playerName)
    {
        Log(playerName + " Finished Loading Level");

        // Register client as finsihed loading the level
        PlayerNames.Remove(playerName);

        // If this is the master client, check to see if everyone is ready and start the game
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            // If all players are ready, start the game!
            if (PlayerNames.Count == 0)
            {
                // Announce that player has loaded the level
                ObjPhotonView.RPC("StartGame", PhotonTargets.All, null);
            }
        }
    }

	private void AddPlayerToGame(int gameID)
	{
		WWWForm form = new WWWForm();
		form.AddField("accountID", PlayerManager.Instance.CurPlayer.AccountID);
		form.AddField("gameID", gameID);
		form.AddField("playerColor", (PlayerManager.Instance.CurPlayer.PlayerColor().r * 255).ToString() + "," + (PlayerManager.Instance.CurPlayer.PlayerColor().g * 255).ToString() + "," + (PlayerManager.Instance.CurPlayer.PlayerColor().b * 255).ToString());
		form.AddField("towerLoadOut", PlayerManager.Instance.CurPlayer.GameLoadOut.GetTowerIDs());
		WWW www = new WWW("http://www.diademstudios.com/outpostdata/GameData_AddPlayerToGame.php", form);
	}
		
	private IEnumerator GetNewGameID()
	{
		int gameID;
		WWWForm form = new WWWForm();
		// When creating a game, track who created it and what level they chose for the game
		form.AddField("accountID", PlayerManager.Instance.CurPlayer.AccountID);
		form.AddField("levelID", MenuManager.Instance.CurrentLevelData.LevelID);
		form.AddField("photonRoomName", SessionManager.Instance.GetCurrentRoomInfo().name.ToString());
		WWW www = new WWW("http://www.diademstudios.com/outpostdata/GameData_StartGame.php", form);
		// Wait until the server has sent data back
		while(!www.isDone)
		{
			yield return 0;
		}

		// Check for an error in processing the GameID before proceeding
		if(string.IsNullOrEmpty(www.error) && !string.IsNullOrEmpty(www.text))
		{
			// Test to see that the server returned a numeric gameID
			if(int.TryParse(www.text, out gameID))
			{
				// Store the gameID as a value in the room so other players can see it
				SessionManager.Instance.SetRoomCustomProperties(new ExitGames.Client.Photon.Hashtable { { "G_ID", gameID } });
			}
			else
			{
				// TODO: Somehow handle invalid GameID
			}
		}
	}
		
	/// <summary>
	/// Determines whether the currently loading level (and all its data) has finished loading
	/// </summary>
	/// <returns><c>true</c> if the loading level is ready; otherwise, <c>false</c>.</returns>
	private IEnumerator IsLevelReady()
	{
		// Save the GameID to the GameManager so that it is remembered for the entire game
		if(GameID != -1)
			GameManager.Instance.GameID = GameID;

		// Loop until all level data has been loaded by the player
		while (!GameManager.Instance.FinishedLoadingLevel())
		{
			yield return 0;
		}

		// Let all clients know that the player has finished loading the level (and all associated level data)
		ObjPhotonView.RPC("LevelLoadingComplete", PhotonTargets.All, SessionManager.Instance.GetPlayerInfo().name);
	}

	/// <summary>
	/// Called when a scene finishes loading
	/// </summary>
	/// <param name="level">Index of loaded scene (found in File->Build Settings...)</param>
	private void OnLevelWasLoaded(int level)
	{
		// This coroutine depends on GameManager - which isn't available until the level has been loaded.
		// After the level is loaded, some additional data might still be loading (EnemySpawnData, etc)
		// This coroutine will continually ask if the player is ready and inform all other clients when the
		// player has finished loading the level and all its data. Once every player is ready, the game starts
		StartCoroutine(IsLevelReady());
	}

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[LoadLevel_Menu] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[LoadLevel_Menu] " + message);
    }

    #endregion MessageHandling
}