using System;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// All public information about a player
/// Owner: Brent Strandy
/// </summary>
[Serializable]
public class ProfileData
{
    public string Username;
	/// <summary>
	/// If the player quit a game before it finished, this is the name of the room they quit.
	/// This value is saved in case they want to join back into the game.
	/// </summary>
	public string PhotonRoomName;
	/// <summary>
	/// If the player quit a game before it finished, this is the TowerLoadOut from that game.
	/// This value is saved in case they want to join back into the game.
	/// </summary>
	public string PreviousTowerLoadOut;

    [HideInInspector]
    [XmlIgnore]
    public bool ShowDebugLogs = true;

    public ProfileData()
    {
    }

	public ProfileData(string username)
    {
        Username = username;
    }

    public ProfileData(ProfileData obj)
    {
		
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[ProfileData] " + message);
    }

    protected void LogError(string message)
    {
        Debug.LogError("[ProfileData] " + message);
    }

    #endregion MessageHandling
}