using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Start_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	public GameObject InputField;
	
	private void OnEnable()
	{
		// Establish listeners for all applicable events
		SessionManager.Instance.OnSMConnected += Connected_Event;

		// TODO: Display a waiting animation to show something is happening
	}
	
	private void OnDisable()
	{
		// Remove listeners for all applicable events
		SessionManager.Instance.OnSMConnected -= Connected_Event;
	}
	
	#region OnClick
	public void Start_Click()
	{
		// Tell the SessionManager the name of the user logging in
		SessionManager.Instance.AuthenticatePlayer(InputField.GetComponent<Text>().text);

		// Tell the MenuManager to transition back
		SessionManager.Instance.StartSession();
	}
	#endregion
	
	#region Events
	private void Connected_Event()
	{
		MenuManager.Instance.ShowMainMenu();
	}
	#endregion
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[Start_Menu] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[Start_Menu] " + message);
	}
	#endregion
}
