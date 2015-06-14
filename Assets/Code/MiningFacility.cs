using UnityEngine;
using System.Collections;

public class MiningFacility : MonoBehaviour
{
	public float IncomeAmountPerSecond { get; private set; }
	public float Health { get; private set; }

	private float LastIncomeTime;
	private PhotonView ObjPhotonView;

	// Use this for initialization
	void Start ()
	{
		IncomeAmountPerSecond = 1.0f;
		LastIncomeTime = Time.time;
		Health = 100.0f;

		// Save reference to PhotonView
		ObjPhotonView = PhotonView.Get (this);
	}

	public void InitializeFromLevelData(LevelData levelData)
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
		EarnIncome();
	}

	public void TakeDamage(float damage)
	{
		// Tell all clients to reduce the health of the mining facility
		ObjPhotonView.RPC("TakeDamageAcrossNetwork", PhotonTargets.All, damage);
	}

	[RPC]
	private void TakeDamageAcrossNetwork(float damage)
	{
		Health -= damage;
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Enemy")
		{
			Health--;
			// Force Enemy to kill itself
			other.gameObject.GetComponent<Enemy>().ForceInstantDeath();
		}
	}

	private void EarnIncome()
	{
		// Only earn income if enough time has passed
		if(Time.time - LastIncomeTime >= 1)
		{
			Player.Instance.EarnIncome(IncomeAmountPerSecond);
			LastIncomeTime = Time.time;
		}
	}
}
