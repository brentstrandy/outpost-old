using System;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// All public information about End Game Player Stats
/// Owner: Brent Strandy
/// </summary>
[Serializable]
public class EndGameStatsData
{
    public string Username;
	/// <summary>
	/// If the player quit a game before it finished, this is the name of the room they quit.
	/// This value is saved in case they want to join back into the game.
	/// </summary>
	public int KillCount;
	/// <summary>
	/// If the player quit a game before it finished, this is the TowerLoadOut from that game.
	/// This value is saved in case they want to join back into the game.
	/// </summary>
	public int Score;

    [HideInInspector]
    [XmlIgnore]
    public bool ShowDebugLogs = true;

	public EndGameStatsData()
    {
    }

	public EndGameStatsData(string username)
    {
        Username = username;
    }

	public EndGameStatsData(EndGameStatsData obj)
    {
		
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
			Debug.Log("[EndGamePlayerStatsData] " + message);
    }

    protected void LogError(string message)
    {
		Debug.LogError("[EndGamePlayerStatsData] " + message);
    }

    #endregion MessageHandling
}