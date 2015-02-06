using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MatchmakingGame_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	public int MaxRejoinRoomAttempts = 3;
	private int RejoinRoomAttempts = 0;
	
	private void OnEnable()
	{
		// Establish listeners for all applicable events
		SessionManager.Instance.OnSMJoinedRoom += JoinedRoom_Event;
		SessionManager.Instance.OnSMRandomJoinFail += RandomJoinFail_Event;
		SessionManager.Instance.OnSMJoinRoomFail += JoinRoomFail_Event;
		SessionManager.Instance.OnSMCreatedRoom += CreatedRoom_Event;
		SessionManager.Instance.OnSMCreateRoomFail += CreateRoomFailed_Event;
		
		// Attempt to join a random room
		this.JoinRandomRoom();
		
		// Always start by allowing the user the max number of Room Join Attempts
		RejoinRoomAttempts = 0;
		
		// TODO: Display a waiting animation to show something is happening
	}
	
	private void OnDisable()
	{
		// Remove listeners for all applicable events
		SessionManager.Instance.OnSMJoinedRoom -= JoinedRoom_Event;
		SessionManager.Instance.OnSMRandomJoinFail -= RandomJoinFail_Event;
		SessionManager.Instance.OnSMJoinRoomFail -= JoinRoomFail_Event;
		SessionManager.Instance.OnSMCreatedRoom -= CreatedRoom_Event;
		SessionManager.Instance.OnSMCreateRoomFail -= CreateRoomFailed_Event;
	}
	
	#region OnClick
	public void Back_Click()
	{
		// Tell the MenuManager to transition back
		MenuManager.Instance.ShowMainMenu();
	}
	#endregion
	
	#region Events
	private void RandomJoinFail_Event(object[] codeAndMsg)
	{
		this.Log("No Rooms Available. Creating Room...");
		// If player is unable to join a random room then create a new room
		SessionManager.Instance.CreateRoom("", new RoomOptions() { isVisible = true, isOpen = true, maxPlayers = 8 }, TypedLobby.Default);
	}
	
	private void JoinRoomFail_Event(object[] codeAndMsg)
	{
		RejoinRoomAttempts++;
		
		// Check to see if we should attempt to join another room
		if(RejoinRoomAttempts <= MaxRejoinRoomAttempts)
		{
			this.LogError("Error joining Room (" + RejoinRoomAttempts + "/" + MaxRejoinRoomAttempts + " attempts). Retrying...");
			this.JoinRandomRoom();
		}
		else
		{
			this.LogError("Error joining Room (" + RejoinRoomAttempts + "/" + MaxRejoinRoomAttempts + " attempts). Abandoning Search.");
			// Tell the MenuManger to transition to the room
			MenuManager.Instance.ShowMainMenu();
		}
	}

	private void CreatedRoom_Event()
	{
		// Tell the MenuManager to transition to the room
		MenuManager.Instance.ShowRoomDetailsMenu();
	}
				
	private void CreateRoomFailed_Event(object[] codeAndMsg)
	{
		this.LogError("Unable to create room.");
		// Tell the MenuManager to transition to the main menu
		MenuManager.Instance.ShowMainMenu();
	}
	
	private void JoinedRoom_Event()
	{
		// Tell the MenuManager to transition to the room
		MenuManager.Instance.ShowRoomDetailsMenu();
	}
	
	private void JoinRandomRoom()
	{
		// TODO: Use predefined options when searching for rooms to find
		SessionManager.Instance.JoinRandomRoom();
	}
	#endregion
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[MatchmakingGame_Menu] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[MatchmakingGame_Menu] " + message);
	}
	#endregion
}
