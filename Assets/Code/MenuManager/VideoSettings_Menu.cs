﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VideoSettings_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	
	private void OnEnable()
	{
		// Establish listeners for all applicable events
	}
	
	private void OnDisable()
	{
		// Remove listeners for all applicable events
	}
	
	#region OnClick
	public void Back_Click()
	{
		// Tell the MenuManager to transition back
		MenuManager.Instance.ShowSettingsMenu();
	}
	#endregion
	
	#region Events
	#endregion
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[VideoSettings_Menu] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[VideoSettings_Menu] " + message);
	}
	#endregion
}
