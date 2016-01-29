using UnityEngine;
using System.Collections;

public class CurrentPlayer : Player
{
	public DataManager<AccountData> AccountDataManager { get; private set; }
	public DataManager<LevelProgressData> LevelProgressDataManager { get; private set; }

	private GameObject PlayerLocator;

	public int Score { get; private set; }
	public float Money { get; private set; }
	public float TotalMoney { get; private set; }
	public int KillCount { get; private set; }

	public PlayerMode Mode;
	public Quadrant CurrentQuadrant;
	public LoadOut GameLoadOut { get; private set; }

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

	public CurrentPlayer() : base()
	{
		AccountDataManager = new DataManager<AccountData>();
		LevelProgressDataManager = new DataManager<LevelProgressData>();

		Score = 0;
		Money = 0.0f;
		TotalMoney = 0.0f;

		Mode = PlayerMode.Selection;
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
