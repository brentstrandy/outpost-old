using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RoomDetails_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	public GameObject[] PlayerName_GUIText;
	public GameObject RoomTitle_GUIInput;
	public GameObject Chat_GUIText;
	public GameObject SendChat_GUIInput;

	private PhotonView photonView;

	public void Start()
	{
		// Save a handle to the photon view associated with this GameObject for use later
		photonView = PhotonView.Get(this);
	}

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
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			RoomTitle_GUIInput.GetComponent<InputField>().enabled = true;
		}
		else
		{
			RoomTitle_GUIInput.GetComponent<InputField>().enabled = false;
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
		photonView.RPC ("StartGame", PhotonTargets.All, null);


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
		// Set the custom properties of a room
		//Room.SetCustomProperties(Hashtable propertiesToSet);
		SessionManager.Instance.GetCurrentRoomInfo().name = RoomTitle_GUIInput.GetComponent<InputField>().text;
	}

	public void SendChat_Click()
	{
		if(SendChat_GUIInput.GetComponent<InputField>().text != "")
		{
			photonView.RPC("SendChatMessage", PhotonTargets.All, SessionManager.Instance.GetPlayerInfo().name, SendChat_GUIInput.GetComponent<InputField>().text);
		}
	}
	#endregion
	
	#region Events
	private void PlayerJoinedRoom_Event(PhotonPlayer player)
	{
		RefreshPlayerNames();
	}

	private void PlayerLeftRoom_Event(PhotonPlayer player)
	{
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

	[RPC]
	void SendChatMessage(string playerName, string msg)
	{
		Chat_GUIText.GetComponent<Text>().text += System.Environment.NewLine + "[" + playerName + "]: " + msg;
		SendChat_GUIInput.GetComponent<InputField>().text = "";
	}

	[RPC]
	void StartGame()
	{
		MenuManager.Instance.ShowStartGame();
	}

	/// <summary>
	/// Refresh the names of all players currently in the room
	/// </summary>
	private void RefreshPlayerNames()
	{
		PhotonPlayer[] playerList = SessionManager.Instance.GetAllPlayersInRoom();

		// Reset all names to blank
		for(int i = 0; i < PlayerName_GUIText.Length; i++)
		{
			if(playerList.Length > i)
				PlayerName_GUIText[i].GetComponent<Text>().text = playerList[i].name;
			else
				PlayerName_GUIText[i].GetComponent<Text>().text = "<OPEN>";
		}
	}

	private void RefreshRoomDetails()
	{
		RoomTitle_GUIInput.GetComponent<InputField>().text = SessionManager.Instance.GetCurrentRoomInfo().name;
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
