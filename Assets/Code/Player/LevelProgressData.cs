using UnityEngine;
using System;
using System.Xml.Serialization;

/// <summary>
/// All information about the progress of a given level
/// Owner: Brent Strandy
/// </summary>
[Serializable]
public class LevelProgressData 
{
	// Level Details
	public int LevelID;
	public int Score;
	public bool Complete;
	
	[HideInInspector] [XmlIgnore]
	public bool ShowDebugLogs = true;
	
	public LevelProgressData() { }

	public LevelProgressData(int levelID, int score, bool complete)
	{
		LevelID = levelID;
		Score = score;
		Complete = complete;
	}

	public LevelProgressData(LevelProgressData obj)
	{
		LevelID = obj.LevelID;
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[LevelProgressData] " + message);
	}
	
	protected void LogError(string message)
	{
		Debug.LogError("[LevelProgressData] " + message);
	}
	#endregion
}