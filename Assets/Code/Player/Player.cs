using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Player is a Singleton used for the entirety of the game session. 
/// It is created when the user starts a game session.
/// </summary>
public class Player : MonoBehaviour
{
	private static Player instance;

	public bool ShowDebugLogs = true;
	public float Money { get; private set; }
	private LoadOut GameLoadOut;

	public void Start()
	{
		Money = 0.0f;
	}

	#region INSTANCE (SINGLETON)
	/// <summary>
	/// Singleton - There can be only one
	/// </summary>
	/// <value>The instance.</value>
	public static Player Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<Player>();
			}
			
			return instance;
		}
	}
	#endregion
	
	public void SetGameLoadOut(LoadOut loadOut)
	{
		GameLoadOut = loadOut;
	}

	public List<TowerData> GetGameLoadOutTowers()
	{
		List<TowerData> towerNames = new List<TowerData>();

		if(GameLoadOut != null)
			towerNames = GameLoadOut.Towers;

		return towerNames;
	}

	public void EarnIncome(float amount)
	{
		Money += amount;
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[Player] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[Player] " + message);
	}
	#endregion
}
