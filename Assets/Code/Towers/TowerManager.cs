using UnityEngine;
using System.Collections;
using Settworks.Hexagons;

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
				var terrain = GameObject.FindObjectOfType<HexMesh>();
				if (terrain != null)
				{
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;
					HexCoord coord;
					if (terrain.IntersectRay(ray, out hit, out coord))
					{
						// TODO: Use the HexCoord to determine the center of the hexagon
						Log("Tower Placement: " + hit.point + " : " + coord);

						// Create a "Look" quaternion that considers the Z axis to be "up" and that faces away from the base
						var rotation = Quaternion.LookRotation(hit.point, new Vector3(0.0f, 0.0f, -1.0f));

						// Send the RPC call to place the tower
						ObjPhotonView.RPC("PlaceTower", PhotonTargets.AllBuffered, "SmallThraceium", hit.point, rotation);

						LastPlacementTime = Time.time;
					}
				}
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
