using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndGameStatsGUI : MonoBehaviour 
{
	public bool ShowDebugLogs = true;

	public Text PlayerNameText;
	public Text PlayerKillCountText;
	public Text PlayerScoreText;

	public void UpdateDetails(string playerName, int killCount, int score)
	{
		PlayerNameText.text = playerName;
		PlayerKillCountText.text = "[ Kill Count : " + killCount.ToString() + " ]";
		PlayerScoreText.text = "[ Score : " + score.ToString() + " ]";
	}

	#region MessageHandling

	protected void Log(string message)
	{
		if (ShowDebugLogs)
			Debug.Log("[EndGameStatsGUI] " + message);
	}

	protected void LogError(string message)
	{
		Debug.LogError("[EndGameStatsGUI] " + message);
	}

	#endregion MessageHandling
}
