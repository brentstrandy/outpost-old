using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoadLevel_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	private List<string> PlayerNames;
	private PhotonView ObjPhotonView;
	
	private bool LevelLoaded;
	private bool DataLoaded;
	
	private void Start()
	{
		// Save a handle to the photon view associated with this GameObject for use later
		ObjPhotonView = PhotonView.Get(this);
	}
	
	private void OnEnable()
	{
		PlayerNames = new List<string>();

		// TO DO: Create a GUI Object for every player, showing their loading status
		foreach(PhotonPlayer player in SessionManager.Instance.GetAllPlayersInRoom())
			PlayerNames.Add(player.name);
	}
	
	private void OnDisable()
	{
		// TO DO: Remove all GUI Objects previously created
		PlayerNames.Clear();
	}
	
	public void Update()
	{
		if(this.enabled)
		{
			// TO DO: Do something interesting for the player to see
		}
	}
	
	[RPC]
	private void StartGame()
	{
		// TO DO: Add screen fade

		// Tell the GameManager to begin playing the game
		GameManager.Instance.StartGame();

		this.gameObject.SetActive(false);
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

	/// <summary>
	/// Determines whether the currently loading level (and all its data) has finished loading
	/// </summary>
	/// <returns><c>true</c> if the loading level is ready; otherwise, <c>false</c>.</returns>
	private IEnumerator IsLevelReady()
	{
		// Loop until all level data has been loaded by the player
		while(!GameManager.Instance.FinishedLoadingLevel())
		{
			yield return 0;
		}
		
		// Let all clients know that the player has finished loading the level (and all associated level data)
		ObjPhotonView.RPC ("LevelLoadingCompete", PhotonTargets.All, SessionManager.Instance.GetPlayerInfo().name);
	}
	
	[RPC]
	private void LevelLoadingCompete(string playerName)
	{
		Log (playerName + " Finished Loading Level");

		// Register client as finsihed loading the level
		PlayerNames.Remove(playerName);
		
		// If this is the master client, check to see if everyone is ready and start the game
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// If all players are ready, start the game!
			if(PlayerNames.Count == 0)
			{
				// Announce that player has loaded the level
				ObjPhotonView.RPC ("StartGame", PhotonTargets.All, null);
			}
		}
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[LoadLevel_Menu] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[LoadLevel_Menu] " + message);
	}
	#endregion
}