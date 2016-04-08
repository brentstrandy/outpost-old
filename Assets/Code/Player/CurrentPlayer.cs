using UnityEngine;
using System.Collections;

public class CurrentPlayer : Player
{
	public DataManager<AccountData> AccountDataManager { get; private set; }
	public DataManager<PlayerLevelProgressData> LevelProgressDataManager { get; private set; }
	public DataManager<PlayerTowerProgressData> TowerProgressDataManager { get; private set; }

	private GameObject PlayerLocator;

	private bool AccountDataDownloaded = false;
	private bool LevelProgressDataDownloaded = false;
	private bool TowerProgressDataDownloaded = false;

	// Player's score in the most recent game
	public int Score { get; private set; }
	// Player's money in the most recent game
	public float Money { get; private set; }
	// Total money earned in the most recent game
	public float TotalMoney { get; private set; }
	// Total number of kills in the most recent game
	public int KillCount { get; private set; }

	public PlayerMode Mode;
	public Quadrant CurrentQuadrant;
	public LoadOut GameLoadOut { get; private set; }

	// Account ID of the local player
	public int AccountID
	{
		get
		{
			if (AccountDataManager.DataList.Count > 0)
				return AccountDataManager.DataList[0].AccountID;
			else
				return 0;
		}
		private set { }
	}

	// Email address of the local player
	public string Email
	{
		get
		{
			if (AccountDataManager.DataList.Count > 0)
				return AccountDataManager.DataList[0].Email;
			else
				return "";
		}
		private set { }
	}

	// If player recently quit a game, this is the room name of that game
	public string RecentlyQuitPhotonRoomName
	{
		get
		{
			if(ProfileDataManager.DataList.Count > 0)
				return ProfileDataManager.DataList[0].PhotonRoomName;
			else
				return "";
		}
		private set { }
	}

	// Delegates
	public delegate void CurrentPlayerActions();
	public event CurrentPlayerActions OnPlayerDataDownloaded;

	public CurrentPlayer(PhotonPlayer photonPlayer) : base(photonPlayer)
	{
		AccountDataManager = new DataManager<AccountData>();
		LevelProgressDataManager = new DataManager<PlayerLevelProgressData>();
		TowerProgressDataManager = new DataManager<PlayerTowerProgressData>();

		Score = 0;
		Money = 0.0f;
		TotalMoney = 0.0f;

		Mode = PlayerMode.Selection;

		// Add event listeners to know when the player's data is done downloading
		AccountDataManager.OnDataLoadSuccess += OnAccountDataDownloaded_Event;
		LevelProgressDataManager.OnDataLoadSuccess += OnLevelProgressDataDownloaded_Event;
		TowerProgressDataManager.OnDataLoadSuccess += OnTowerProgressDataDownloaded_Event;
	}

	#region EVENTS

	private void OnAccountDataDownloaded_Event()
	{
		Log("Downloaded Account Data. Username: " + Username);
		AccountDataDownloaded = true;

		CheckAllDataDownloaded();
	}

	protected override void OnProfileDataDownloaded_Event()
	{
		Log("Downloaded Profile Data. Username: " + Username);
		ProfileDataDownlaoded = true;

		CheckAllDataDownloaded();
	}

	private void OnLevelProgressDataDownloaded_Event()
	{
		Log("Downloaded Level Progress Data. Username: " + Username);
		LevelProgressDataDownloaded = true;

		CheckAllDataDownloaded();
	}

	private void OnTowerProgressDataDownloaded_Event()
	{
		Log("Downloaded Tower Progress Data. Username: " + Username);
		TowerProgressDataDownloaded = true;

		CheckAllDataDownloaded();
	}

	#endregion
	/// <summary>
	/// Checks to see if all of the player's data has downloaded. If downloaded, it will broadcast a message saying all
	/// data has successfully downloaded
	/// </summary>
	private void CheckAllDataDownloaded()
	{
		// Check to see if all data has been downloaded
		if(AccountDataDownloaded && ProfileDataDownlaoded && LevelProgressDataDownloaded && TowerProgressDataDownloaded)
		{
			// Broadcast event that all of the player's data has been downloaded.
			if(OnPlayerDataDownloaded != null)
				OnPlayerDataDownloaded();
		}
	}

	public void PurchaseTower(int price)
	{
		Money -= price;
	}

	public void EarnIncome(float income)
	{
		Money += income;
		TotalMoney += income;
	}

	public void ProccessKill(int points)
	{
		Score += points;
		KillCount++;
	}

	public void SetGameLoadOut(LoadOut loadOut)
	{
		GameLoadOut = loadOut;
	}

	public void SetStartingMoney(float amount)
	{
		Money = amount;
		TotalMoney = amount;
	}

	public void ResetData()
	{
		GameLoadOut = null;
		Money = 0.0f;
		TotalMoney = 0.0f;
		Score = 0;
	}

	#region MessageHandling

	protected override void Log(string message)
	{
		if (ShowDebugLogs)
			Debug.Log("[Player] " + message);
	}

	protected override void LogError(string message)
	{
		Debug.LogError("[Player] " + message);
	}

	#endregion MessageHandling
}
