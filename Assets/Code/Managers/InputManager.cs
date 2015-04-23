using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour
{
	private static InputManager instance;
	public bool ShowDebugLogs = true;

	#region EVENTS (DELEGATES)
	public delegate void QuadrantAction(string direction);
	public event QuadrantAction OnQuadrantRotate;
	#endregion

	#region INSTANCE (SINGLETON)
	/// <summary>
	/// Singleton - There can only be one
	/// </summary>
	/// <value>The instance.</value>
	public static InputManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<InputManager>();
			}
			
			return instance;
		}
	}
	
	void Awake()
	{
		instance = this;
	}
	#endregion

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Only check for input if the game is currently running
		if(GameManager.Instance.GameRunning)
		{
			// Test for a change in the player's current quadrant
			if(Input.GetKeyDown("left"))
			{
				if(OnQuadrantRotate != null)
					OnQuadrantRotate("left");
			}
			else if(Input.GetKeyDown("right"))
			{
				if(OnQuadrantRotate != null)
					OnQuadrantRotate("right");
			}
		}
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[InputManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[InputManager] " + message);
	}
	#endregion
}
