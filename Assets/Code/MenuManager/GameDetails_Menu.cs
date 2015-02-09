using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameDetails_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	public GameObject[] PlayerNameText;
	public GameObject RoomTitleText;
	
	private void OnEnable()
	{
		// Establish listeners for all applicable events
		SessionManager.Instance.OnSMJoinedRoom += JoinedRoom_Event;
		SessionManager.Instance.OnSMPlayerJoinedRoom += PlayerJoinedRoom_Event;
		SessionManager.Instance.OnSMPlayerLeftRoom += PlayerLeftRoom_Event;

		// Immediately refresh the player name list to show the host
		RefreshPlayerNames();
		
		// Immediately refresh the room details
		RefreshRoomDetails();

		// See if the player is currently the room owner or just joining
		if(MenuManager.Instance.RoomHost)
		{
			RoomTitleText.GetComponent<InputField>().enabled = true;
		}
		else
		{
			RoomTitleText.GetComponent<InputField>().enabled = false;
		}
	}
	
	private void OnDisable()
	{
		// Remove listeners for all applicable events
		SessionManager.Instance.OnSMJoinedRoom -= JoinedRoom_Event;
		SessionManager.Instance.OnSMPlayerJoinedRoom -= PlayerJoinedRoom_Event;
		SessionManager.Instance.OnSMPlayerLeftRoom -= PlayerLeftRoom_Event;
	}
	
	#region OnClick
	public void StartGame_Click()
	{
		// TO DO: Start the game
	}

	public void Back_Click()
	{
		// TO DO: Ask the user if they're sure they want to leave

		SessionManager.Instance.LeaveRoom();

		// Tell the MenuManager to transition back
		MenuManager.Instance.ShowMainMenu();
	}

	public void RoomTitle_AfterUpdate()
	{
		this.Log("updating title");
		SessionManager.Instance.GetCurrentRoomInfo().name = RoomTitleText.GetComponent<InputField>().text;
	}
	#endregion
	
	#region Events
	private void PlayerJoinedRoom_Event(PhotonPlayer player)
	{
		this.Log("Player Joined Room: " + player.name);

		RefreshPlayerNames();
	}

	private void PlayerLeftRoom_Event(PhotonPlayer player)
	{
		this.Log("Player Left Room: " + player.name);
		
		RefreshPlayerNames();
	}

	private void JoinedRoom_Event()
	{
		// Immediately refresh the player name list to show the host
		RefreshPlayerNames();
		
		// Immediately refresh the room details
		RefreshRoomDetails();
	}
	#endregion

	/// <summary>
	/// Refresh the names of all players currently in the room
	/// </summary>
	private void RefreshPlayerNames()
	{
		PhotonPlayer[] playerList = SessionManager.Instance.GetAllPlayersInRoom();

		// Reset all names to blank
		for(int i = 0; i < PlayerNameText.Length; i++)
		{
			if(playerList.Length > i)
				PlayerNameText[i].GetComponent<Text>().text = playerList[i].name;
			else
				PlayerNameText[i].GetComponent<Text>().text = "<OPEN>";
		}
	}

	private void RefreshRoomDetails()
	{
		RoomTitleText.GetComponent<InputField>().text = SessionManager.Instance.GetCurrentRoomInfo().name;
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[GameDetails_Menu] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[GameDetails_Menu] " + message);
	}
	#endregion
}
