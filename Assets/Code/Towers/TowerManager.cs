using UnityEngine;
using System.Collections;

public class TowerManager : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	public GameObject SmallThraceium;

	private PhotonView ObjPhotonView;
	private double LastPlacementTime;
	private Vector3 MouseClickPosition;

	// Use this for initialization
	void Start ()
	{
		// Save a handle to the photon view associated with this GameObject for use later
		ObjPhotonView = PhotonView.Get(this);

		LastPlacementTime = Time.time;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Input.GetMouseButtonDown(0))
		{
			if(Time.time - LastPlacementTime > 1)
			{
				MouseClickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

				//ObjPhotonView.RPC("PlaceTower", PhotonTargets.AllBuffered, "SmallThraceium", new Vector3(MouseClickPosition.x, 0, MouseClickPosition.z), Quaternion.identity);

				LastPlacementTime = Time.time;

			}
		}
	}

	[RPC]
	public void PlaceTower(string towerName, Vector3 position, Quaternion rotation)
	{
		Instantiate(SmallThraceium, position, rotation);
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[TowerManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[TowerManager] " + message);
	}
	#endregion
}
