using UnityEngine;
using System.Collections;

public class MiningFacility : MonoBehaviour
{
	public float IncomeAmountPerSecond { get; private set; }
	public float Health { get; private set; }

	private float LastIncomeTime;

	// Use this for initialization
	void Start ()
	{
		IncomeAmountPerSecond = 1.0f;
		LastIncomeTime = Time.time;
		Health = 100.0f;
	}
	
	// Update is called once per frame
	void Update ()
	{
		EarnIncome();
	}

	public void TakeDamage(float damage)
	{
		Health -= damage;
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Enemy")
		{
			Health--;
			SessionManager.Instance.DestroyObject(other.gameObject);
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
