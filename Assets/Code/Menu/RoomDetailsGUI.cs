using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RoomDetailsGUI : MonoBehaviour 
{
	public bool ShowDebugLogs = true;

	public Button JoinButton;
	public Text RoomNameText;
	public Text PlayerCountText;
	public Text StatusText;

	public void UpdateDetails(string roomName, int totalPlayers, int maxPlayers, string status)
	{
		RoomNameText.text = roomName;
		PlayerCountText.text = "[" + totalPlayers.ToString() + " / " + maxPlayers.ToString() + "]";
		StatusText.text = status;
	}

	#region MessageHandling

	protected void Log(string message)
	{
		if (ShowDebugLogs)
			Debug.Log("[JoinGame_Menu] " + message);
	}

	protected void LogError(string message)
	{
		Debug.LogError("[JoinGame_Menu] " + message);
	}

	#endregion MessageHandling
}
