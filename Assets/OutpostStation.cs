using UnityEngine;
using System.Collections;

public class OutpostStation : MonoBehaviour
{
	public float IncomeAmountPerSecond { get; private set; }
	public float Health { get; private set; }

	private float LastIncomeTime;

	// Use this for initialization
	void Start ()
	{
		IncomeAmountPerSecond = 0.5f;
		LastIncomeTime = Time.time;
		Health = 100.0f;
	}
	
	// Update is called once per frame
	void Update ()
	{
		EarnIncome();
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Enemy")
			SessionManager.Instance.DestroyObject(other.gameObject);
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
