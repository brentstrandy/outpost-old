using UnityEngine;
using System.Collections;

public class HAM_Explosion : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
		
	}

	public void SetData()
	{

	}

	private void OnTriggerEnter(Collider other)
	{
		// Only take action if this is the Master Client
		if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Only affect Enemies
			if (other.tag == "Enemy")
			{
				// Load attributes of the Hammer Tower
				TowerData HAM_Data = GameDataManager.Instance.FindTowerDataByDisplayName("Hammer Tower");
				// Damage the enemy
				other.gameObject.GetComponent<Enemy>().TakeDamage(HAM_Data.BallisticDamage, HAM_Data.ThraceiumDamage, SessionManager.Instance.GetPlayerInfo());
			}
		}
	}
}
