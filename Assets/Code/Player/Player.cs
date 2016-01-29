using UnityEngine;
using System.Collections;

public class Player
{
	public bool ShowDebugLogs = true;
    
	public DataManager<ProfileData> ProfileDataManager { get; private set; }
	public bool Connected { get; private set; }
	private float DisconnectedTimestamp = -1;

	public string Username
	{
		get
		{
			if (ProfileDataManager.DataList.Count > 0)
				return ProfileDataManager.DataList[0].Username;
			else
				return "";
		}
		private set { }
	}

	public Player()
    {
        // Download profile data from the server
		ProfileDataManager = new DataManager<ProfileData>();
    }

	public void DisconnectPlayer()
	{
		Connected = false;
		DisconnectedTimestamp = Time.time;
	}

	public void ReconnectPlayer()
	{
		Connected = true;
		DisconnectedTimestamp = -1;
	}

	public float DisconnectedDuration()
	{
		if(DisconnectedTimestamp == -1)
			return 0;
		else
			return Time.time - DisconnectedTimestamp;
	}

	#region MessageHandling

	protected virtual void Log(string message)
	{
		if (ShowDebugLogs)
			Debug.Log("[Player] " + message);
	}

	protected virtual void LogError(string message)
	{
		Debug.LogError("[Player] " + message);
	}

	#endregion MessageHandling
}
