﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadGameInProgressLevel_Menu : MonoBehaviour
{
    public bool ShowDebugLogs = true;
    //private PhotonView ObjPhotonView;

    //public GameObject Level_GUIText;

	//private int GameID;

    private void Start()
    {
        // Save a handle to the photon view associated with this GameObject for use later
        //ObjPhotonView = PhotonView.Get(this);

		//GameID = (int)SessionManager.Instance.GetRoomCustomProperties()["G_ID"];

    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
		
    }

    public void Update()
    {
        if (this.enabled)
        {
            // TO DO: Do something interesting for the player to see
        }
    }
	/*
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
	
	[PunRPC]
	private void OnGameIDCreated(int gameID)
	{
		GameID = gameID;

		Log("Acquired GameID: " + gameID.ToString());

		// If the GameManager has already been created, save the GameID to the GameManager so that it is remembered for the entire game
		if(GameManager.Instance != null)
			GameManager.Instance.GameID = GameID;

		// Inform the server that you (the player) is in the game
		AddPlayerToGame(GameID);
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
*/
    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[LoadGameInProgressLevel_Menu] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
			Debug.LogError("[LoadGameInProgressLevel_Menu] " + message);
    }

    #endregion MessageHandling
}