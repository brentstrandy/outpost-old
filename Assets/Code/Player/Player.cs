using UnityEngine;
using System.Collections;

public class Player
{
	public bool ShowDebugLogs = true;
    
	public DataManager<ProfileData> ProfileDataManager { get; private set; }
	public bool Connected { get; private set; }
	private float DisconnectedTimestamp = -1;

	private PhotonPlayer PhotonPlayerInfo;

	public string Username
	{
		get
		{
			if (PhotonPlayerInfo != null && PhotonPlayerInfo.name != "")
				return PhotonPlayerInfo.name;
			else
				return "";
		}
		private set { }
	}

	public Player(PhotonPlayer photonPlayer)
    {
		PhotonPlayerInfo = photonPlayer;

        // Download profile data from the server
		ProfileDataManager = new DataManager<ProfileData>();

		ProfileDataManager.OnDataLoadSuccess += OnProfileDataDownloaded;
		ProfileDataManager.OnDataLoadFailure += OnProfileDataDownloaded;
    }

	public Color PlayerColor()
	{
		if(PhotonPlayerInfo.customProperties["PlayerColorIndex"] == null)
			return Color.clear;
		else
			return PlayerColors.colors[(int)PhotonPlayerInfo.customProperties["PlayerColorIndex"]];
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

	private void OnProfileDataDownloaded()
	{
		Log("Downloaded Profile Data! Username: " + Username);
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
