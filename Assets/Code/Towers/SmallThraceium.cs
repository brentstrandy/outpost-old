using UnityEngine;
using System.Collections;

public class SmallThraceium : Tower {

	// Use this for initialization
	public override void Start()
	{
		Name = "Small Thraceium";
	}
	
	// Update is called once per frame
	public override void Update()
	{

	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[SmallThraceium] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[SmallThraceium] " + message);
	}
	#endregion
}
