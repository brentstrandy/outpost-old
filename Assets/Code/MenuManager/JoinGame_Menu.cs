using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class JoinGame_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	public GameObject RoomDetailsGUI_Prefab;

	private List<GameObject> RoomDetailsGUIList = new List<GameObject>();

	private void OnEnable()
	{
		// Establish listeners for all applicable events
		SessionManager.Instance.OnSMReceivedRoomListUpdate += RoomListUpdated_Event;;
		SessionManager.Instance.OnSMJoinRoomFail += JoinRoomFail_Event;
		SessionManager.Instance.OnSMJoinedRoom += JoinedRoom_Event;

		// Update the current list of rooms available to choose from
		UpdateRoomList();

		// TODO: Display a waiting animation to show something is happening
	}
	
	private void OnDisable()
	{
		// Remove listeners for all applicable events
		SessionManager.Instance.OnSMReceivedRoomListUpdate -= RoomListUpdated_Event;
		SessionManager.Instance.OnSMJoinRoomFail -= JoinRoomFail_Event;
		SessionManager.Instance.OnSMJoinedRoom -= JoinedRoom_Event;
	}
	
	#region OnClick
	public void Back_Click()
	{
		// Tell the MenuManager to transition back
		MenuManager.Instance.ShowMainMenu();
	}

	public void JoinRoom_Click(string buttonText)
	{
		SessionManager.Instance.JoinRoom(buttonText);
	}
	#endregion
	
	#region Events
	private void JoinRoomFail_Event(object[] codeAndMsg)
	{
		// TO DO: Refresh the list of rooms
	}
	
	private void JoinedRoom_Event()
	{
		// Tell the MenuManager to transition to the room
		MenuManager.Instance.ShowRoomDetailsMenu();
	}
	
	private void RoomListUpdated_Event()
	{
		UpdateRoomList();
	}
	#endregion

	/// <summary>
	/// Updates the UI displaying the current list of joinable rooms
	/// </summary>
	private void UpdateRoomList()
	{
		int index = 0;

		// Before updating the room list, destroy the current list
		DestroyRoomDetailGUIPrefabs();

		// Get a list of all joinable rooms
		RoomInfo[] roomList = SessionManager.Instance.GetRoomList();

		// Display each room currently in the lobby
		foreach(RoomInfo room in roomList)
		{
			// Instantiate row for each room and add it as a child of the JoinGame UI Panel
			GameObject obj = Instantiate(RoomDetailsGUI_Prefab) as GameObject;
			obj.GetComponentInChildren<Text>().text = room.name;
			obj.GetComponent<Button>().onClick.AddListener(() => JoinRoom_Click(obj.GetComponentInChildren<Text>().text));
			obj.transform.SetParent(this.transform);
			obj.transform.localScale = new Vector3(1, 1, 1);
			obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -70 + (-35 * index));
			obj.GetComponent<RectTransform>().localPosition = new Vector3(obj.GetComponent<RectTransform>().localPosition.x, obj.GetComponent<RectTransform>().localPosition.y, 0);
			obj.transform.rotation = new Quaternion(0, 0, 0, 0);

			// Create a handle to all the prefabs that were created so we can destroy them later
			RoomDetailsGUIList.Add(obj);

			index++;
		}
	}

	/// <summary>
	/// Destroys all prefabs created for list of joinable rooms
	/// </summary>
	private void DestroyRoomDetailGUIPrefabs()
	{
		foreach(GameObject obj in RoomDetailsGUIList)
			Destroy (obj);
	}

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
