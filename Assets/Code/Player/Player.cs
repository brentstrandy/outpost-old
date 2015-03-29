using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;

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
	private double LastTowerPlacementTime;
	private TowerData PlacementTowerData;
	private HexMesh TerrainMesh;

	public void Start()
	{
		Money = 0.0f;
		PlacementTowerData = null;
		LastTowerPlacementTime = Time.time;
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

	void Awake()
	{
		instance = this;
	}
	#endregion
	
	public void SetGameLoadOut(LoadOut loadOut)
	{
		GameLoadOut = loadOut;
	}

	public void Update()
	{
		// TO DO: Do not perform this action if the player has clicked a button, tower, or any other object besides a blank Hex tile
		// Check for player input (mouse click)
		if(Input.GetMouseButtonDown(0))
		{
			// Check to see if a tower has been selected by the player to be placed
			if(PlacementTowerData != null)
			{
				// Ensure towers cannot be repeatedly placed every frame
				if(Time.time - LastTowerPlacementTime > 1)
				{
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;
					HexCoord coord;

					// Test to see if the player's click intersected with the Terrain (HexMesh)
					if (TerrainMesh.IntersectRay(ray, out hit, out coord))
					{
						// TODO: Use the HexCoord to determine the center of the hexagon
						Log("Tower Placement: " + hit.point + " : " + coord);
						
						// Create a "Look" quaternion that considers the Z axis to be "up" and that faces away from the base
						var rotation = Quaternion.LookRotation(hit.point, new Vector3(0.0f, 0.0f, -1.0f));
						
						// Tell all players to instantiate a tower
						SessionManager.Instance.InstantiateObject("Towers/" + PlacementTowerData.PrefabName, hit.point, rotation);
						
						LastTowerPlacementTime = Time.time;
					}
				}
			}
		}
	}

	public void TowerSelectedForPlacement(TowerData towerData)
	{
		PlacementTowerData = towerData;
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

	public void ResetData()
	{
		GameLoadOut = null;
		Money = 0.0f;
	}

	public void OnLevelWasLoaded(int level)
	{
		// TerrainMesh must be set when the level is started because the HexMesh object is not created
		// until the level loads. All levels MUST begin with a defined prefix for this to work properly
		if(Application.loadedLevelName.StartsWith("Level"))
			TerrainMesh = GameObject.FindGameObjectWithTag("Terrain").GetComponent<HexMesh>() as HexMesh;
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
