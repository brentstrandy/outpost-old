using UnityEngine;
using System.Collections;

public class MiningFacility : Tower
{
	private static MiningFacility instance;

	public float IncomePerSecond { get; private set; }

	private float LastIncomeTime;

	// Use this for initialization
	public override void Start ()
	{
		IncomePerSecond = 1.0f;
		LastIncomeTime = Time.time;
		Health = 100.0f;

		// Save reference to PhotonView
		ObjPhotonView = PhotonView.Get (this);
		NetworkViewID = ObjPhotonView.viewID;
	}

	public void InitializeFromLevelData(LevelData levelData)
	{
		// Track the newly added tower in the TowerManager
		GameManager.Instance.TowerManager.AddActiveTower(this);

		IncomePerSecond = levelData.IncomePerSecond;
		Health = levelData.MiningFacilityHealth;
		Player.Instance.SetStartingMoney(levelData.StartingMoney);
	}
	
	// Update is called once per frame
	public override void Update ()
	{
		EarnIncome();
	}

	protected override void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Enemy")
		{
			// Deal damage to the Mining Facility
			this.TakeDamage(other.gameObject.GetComponent<Enemy>().EnemyAttributes.DamageDealt);
			// Force Enemy to kill itself
			other.gameObject.GetComponent<Enemy>().ForceInstantDeath();
		}
	}

	protected override void OnTriggerStay(Collider other)
	{

	}

	protected override void OnTriggerExit(Collider other)
	{

	}

	private void EarnIncome()
	{
		// Only earn income if enough time has passed
		if(Time.time - LastIncomeTime >= 1)
		{
			Player.Instance.EarnIncome(IncomePerSecond);
			LastIncomeTime = Time.time;
		}
	}
}
