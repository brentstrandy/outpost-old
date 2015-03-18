using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndGame_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	public GameObject EndGameText;

	private void OnEnable()
	{
		// Display the proper EndGame message depending on whether the player(s) won or lost
		if(GameManager.Instance.Victory)
		{
			EndGameText.GetComponentInChildren<Text>().text = "VICTORY!";
		}
		else
		{
			EndGameText.GetComponentInChildren<Text>().text = "DEFEAT :(";
		}
	}

	private void OnDisable()
	{

	}

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	#region OnClick
	public void Continue_Click()
	{
		// Reset player Loadout data
		Player.Instance.ResetData();

		MenuManager.Instance.ReturnToRoomDetailsMenu();
	}
	#endregion

	#region Events
	#endregion

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[EndGame_Menu] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[EndGame_Menu] " + message);
	}
	#endregion
}
