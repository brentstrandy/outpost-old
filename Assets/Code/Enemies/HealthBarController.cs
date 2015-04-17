using UnityEngine;
using System.Collections;

public class HealthBarController : MonoBehaviour
{
	private Camera MainCamera;

	public bool ShowDebugLogs = true;
	public int HealthBarSize = 100;
	public GameObject NegativeBar;
	public GameObject HealthBar;

	private float MaxHealth;

	// Use this for initialization
	void Start ()
	{
		MainCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.rotation = MainCamera.transform.rotation;
	}

	public void InitializeBars(float maxHealth)
	{
		MaxHealth = maxHealth;
	}

	public void UpdateHealthBar(float newHealth)
	{
		// The SpriteRenderer is 1x1 pixel, therefore we must multiply the pixel 
		HealthBar.transform.localScale = new Vector3(newHealth / MaxHealth * HealthBarSize, HealthBar.transform.localScale.y, HealthBar.transform.localScale.z);
	}

	#region MessageHandling
	private void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[HealthBarController] " + message);
	}
	
	private void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[HealthBarController] " + message);
	}

	private void LogWarning(string message)
	{
		if(ShowDebugLogs)
			Debug.LogWarning("[HealthBarController] " + message);
	}
	#endregion
}
