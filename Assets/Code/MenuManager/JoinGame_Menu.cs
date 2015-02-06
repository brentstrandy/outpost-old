using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class JoinGame_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	public int MaxRejoinRoomAttempts = 3;
	private int RejoinRoomAttempts = 0;
	
	private void OnEnable()
	{
		// Establish listeners for all applicable events
		SessionManager.Instance.OnSMReceivedRoomListUpdate += RoomListUpdated_Event;;
		SessionManager.Instance.OnSMJoinRoomFail += JoinRoomFail_Event;
		
		// TODO: Display a waiting animation to show something is happening
	}
	
	private void OnDisable()
	{
		// Remove listeners for all applicable events
		SessionManager.Instance.OnSMReceivedRoomListUpdate -= RoomListUpdated_Event;
		SessionManager.Instance.OnSMJoinRoomFail -= JoinRoomFail_Event;
	}
	
	#region OnClick
	public void Back_Click()
	{
		// Tell the MenuManager to transition back
		MenuManager.Instance.ShowMainMenu();
	}
	#endregion
	
	#region Events
	private void JoinRoomFail_Event(object[] codeAndMsg)
	{

	}
	
	private void JoinedRoom_Event()
	{
		// Tell the MenuManager to transition to the room
		MenuManager.Instance.ShowRoomDetailsMenu();
	}
	
	private void RoomListUpdated_Event()
	{

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
