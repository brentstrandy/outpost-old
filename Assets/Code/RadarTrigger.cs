using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class RadarTrigger : MonoBehaviour
{
	public GameObject EnemyRadarBar;
	public GameObject AllyRadarBar;
	public GameObject PlayerRadarBar;

	private float LastEnemyFoundTime;
	private float LastAllyFoundTime;
	private float LastPlayerFoundTime;
	
	// Use this for initialization
	void Start ()
	{
		LastEnemyFoundTime = Time.time;
		LastAllyFoundTime = Time.time;
		LastPlayerFoundTime = Time.time;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Time.time - LastAllyFoundTime <= 0.1f)
			AllyRadarBar.GetComponent<Image>().color = Color.blue;
		else
			AllyRadarBar.GetComponent<Image>().color = Color.white;

		if(Time.time - LastEnemyFoundTime <= 0.1f)
			EnemyRadarBar.GetComponent<Image>().color = Color.red;
		else
			EnemyRadarBar.GetComponent<Image>().color = Color.white;

		if(Time.time - LastPlayerFoundTime <= 0.1f)
			PlayerRadarBar.GetComponent<Image>().color = Color.yellow;
		else
			PlayerRadarBar.GetComponent<Image>().color = Color.white;
	}

	void OnTriggerStay(Collider other)
	{
		if(other.tag == "Enemy")
		{
			LastEnemyFoundTime = Time.time;
		}
		else if(other.tag == "Player")
		{
			// Client does NOT see host's location

			// We only care about other players, not the current player
			if(other.name != SessionManager.Instance.GetPlayerInfo().name)
				LastAllyFoundTime = Time.time;
			else
				LastPlayerFoundTime = Time.time;
		}
	}
}
