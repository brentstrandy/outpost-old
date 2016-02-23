using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InGamePause_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	public Button ReturnToGameButton;
	public Button QuitButton;
	public Button YesButton;

	private void OnEnable()
	{
		YesButton.gameObject.SetActive(false);
		QuitButton.gameObject.SetActive(true);
	}

	#region OnClick

	public void ReturnToGame_Click()
	{
		// Hide the In Game Pause Menu
		MenuManager.Instance.ToggleInGamePauseMenu();
	}

	public void Quit_Click()
	{
		// Show double-confirmation
		YesButton.gameObject.SetActive(true);
		QuitButton.gameObject.SetActive(false);
	}

	public void Yes_Click()
	{
		// Hide the In Game Pause Menu
		MenuManager.Instance.ToggleInGamePauseMenu();

		// End the game
		GameManager.Instance.EndGame_Quit();
	}

	#endregion

	#region MessageHandling

	protected void Log(string message)
	{
		if (ShowDebugLogs)
			Debug.Log("[InGamePause_Menu] " + message);
	}

	protected void LogError(string message)
	{
		if (ShowDebugLogs)
			Debug.LogError("[InGamePause_Menu] " + message);
	}

	#endregion MessageHandling
}
