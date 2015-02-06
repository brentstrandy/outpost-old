using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameDetails_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	
	private void OnEnable()
	{
		// Establish listeners for all applicable events
		SessionManager.Instance.OnSMPlayerJoinedRoom += PlayerJoinedRoom_Event;
		
		// TODO: Display a waiting animation to show something is happening
	}
	
	private void OnDisable()
	{
		// Remove listeners for all applicable events
		SessionManager.Instance.OnSMPlayerJoinedRoom -= PlayerJoinedRoom_Event;
	}
	
	#region OnClick
	public void Back_Click()
	{
		// TODO: Ask the user if they're sure they want to leave

		SessionManager.Instance.LeaveRoom();

		// Tell the MenuManager to transition back
		MenuManager.Instance.ShowMainMenu();
	}
	#endregion
	
	#region Events
	private void PlayerJoinedRoom_Event(PhotonPlayer player)
	{
		this.Log("Player Joined Room: " + player.name);
	}
	#endregion
	
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
