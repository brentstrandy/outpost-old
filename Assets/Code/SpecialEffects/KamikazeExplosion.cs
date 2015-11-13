using UnityEngine;
using System.Collections;

public class KamikazeExplosion : MonoBehaviour
{
	EnemyData KamikazeData;

	// Use this for initialization
	void Start ()
	{
		// Load attributes of Kamikaze explosion
		KamikazeData = GameDataManager.Instance.FindEnemyDataByDisplayName("Kamikaze");
		this.GetComponent<Renderer>().material.color = KamikazeData.HighlightColor;
	}

	private void OnTriggerEnter(Collider other)
	{
		// Only take action if this is the Master Client
		if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Only affect Enemies
			if (other.tag == "Tower" || other.tag == "Mining Facility")
			{
				// Tell enemy to take damage (only the Master Client can do this)
				if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
				{
					// Damage the tower
					other.gameObject.GetComponent<Tower>().TakeDamage(KamikazeData.BallisticDamage, KamikazeData.ThraceiumDamage);
				}
			}
		}
	}
}
