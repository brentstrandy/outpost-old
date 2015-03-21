﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Start_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	public GameObject InputField;
	public GameObject StartButton;
	public GameObject CannotConnectText;
	public GameObject RetryButton;
	public GameObject OfflineButton;

	private void OnEnable()
	{
		// Establish listeners for all applicable events
		SessionManager.Instance.OnSMConnected += Connected_Event;
		SessionManager.Instance.OnSMConnectionFail += ConnectionFailed_Event;

		// TODO: Display a waiting animation to show something is happening
	}
	
	private void OnDisable()
	{
		// Remove listeners for all applicable events
		SessionManager.Instance.OnSMConnected -= Connected_Event;
		SessionManager.Instance.OnSMConnectionFail -= ConnectionFailed_Event;
	}
	
	#region OnClick
	public void Start_Click()
	{
		// Tell the SessionManager the name of the user logging in
		SessionManager.Instance.AuthenticatePlayer(InputField.GetComponentInChildren<Text>().text);

		// Tell the SessionManager to start a new session with the player's credentials
		SessionManager.Instance.StartSession();
	}

	public void Retry_Click()
	{
		Start_Click();
	}

	public void Offline_Click()
	{
		// Go into Offline mode and open the main menu
		SessionManager.Instance.SetOfflineMode(true);
		MenuManager.Instance.ShowMainMenu();
	}
	#endregion
	
	#region Events
	private void Connected_Event()
	{
		MenuManager.Instance.ShowMainMenu();
	}

	private void ConnectionFailed_Event(DisconnectCause cause)
	{
		InputField.SetActive(false);
		StartButton.SetActive(false);
		CannotConnectText.SetActive(true);
		RetryButton.SetActive(true);
		OfflineButton.SetActive(true);
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
