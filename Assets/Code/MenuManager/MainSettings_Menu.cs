using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MainSettings_Menu : MonoBehaviour
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
	public void AudioSettings_Click()
	{
		MenuManager.Instance.ShowAudioSettingsMenu();
	}

	public void VideoSettings_Click()
	{	
		MenuManager.Instance.ShowVideoSettingsMenu();
	}

	public void ControlSettings_Click()
	{	
		MenuManager.Instance.ShowControlSettingsMenu();
	}

	public void GameplaySettings_Click()
	{	
		MenuManager.Instance.ShowGameplaySettingsMenu();
	}

	public void Back_Click()
	{	
		// Tell the MenuManager to transition back
		MenuManager.Instance.ShowMainMenu();
	}
	#endregion
	
	#region Events
	#endregion
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[MainSettings_Menu] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[MainSettings_Menu] " + message);
	}
	#endregion
}
