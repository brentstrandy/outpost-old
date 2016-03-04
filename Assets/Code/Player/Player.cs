using UnityEngine;
using System.Collections;

public class Player
{
	public bool ShowDebugLogs = true;
    
	public DataManager<ProfileData> ProfileDataManager { get; private set; }
	public bool Connected { get; private set; }
	private float DisconnectedTimestamp = -1;
	protected bool ProfileDataDownlaoded = false;

	private PhotonPlayer PhotonPlayerInfo;

	// Username of the local player
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
		Connected = true;

        // Download profile data from the server
		ProfileDataManager = new DataManager<ProfileData>();

		ProfileDataManager.OnDataLoadSuccess += OnProfileDataDownloaded_Event;
		// TODO: Add conditions for when the data did not download - OnDataLoadFailure
    }

	#region EVENTS

	protected virtual void OnProfileDataDownloaded_Event()
	{
		Log("Downloaded Profile Data! Username: " + Username);
		ProfileDataDownlaoded = true;
	}

	#endregion

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

	public void ReconnectPlayer(PhotonPlayer newPhotonPlayer)
	{
		PhotonPlayerInfo = newPhotonPlayer;
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
