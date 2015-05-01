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
	public Quadrant CurrentQuadrant;

	private GameObject PlayerLocator;
	private string Name;
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
					// Make sure the player has enough money to place the tower
					if(this.Money >= PlacementTowerData.InstallCost)
					{
						Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
						RaycastHit hit;
						HexCoord coord;

						// Test to see if the player's click intersected with the Terrain (HexMesh)
						if (TerrainMesh.IntersectRay(ray, out hit, out coord) && TerrainMesh.InPlacementRange(coord))
						{
							// Create a "Look" quaternion that considers the Z axis to be "up" and that faces away from the base
							var rotation = Quaternion.LookRotation(hit.point, new Vector3(0.0f, 0.0f, -1.0f));
							
							// Tell all players to instantiate a tower
							SessionManager.Instance.InstantiateObject("Towers/" + PlacementTowerData.PrefabName, hit.point, rotation);
							
							LastTowerPlacementTime = Time.time;

							// Charge the player for building the tower
							this.Money -= PlacementTowerData.InstallCost;
						}
					}
					else
					{
						// This currently does not work. My intent is to display this message alongside the place where the player clicks
						/*
						var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
						var rotation = Quaternion.LookRotation(position, new Vector3(0.0f, 0.0f, -1.0f));

						// Display Insufficient Capital
						GameObject go = Instantiate(Resources.Load("Utilities/InsufficientCapital"), position, rotation) as GameObject;
						go.transform.SetParent(GameObject.Find("InGame Canvas").transform, false);
						*/
					}
				}
			}
		}

		// Always keep the player locator GameObject in front of the camera. The GameObject has a PhotonView attached to it for the RadarManager
		// to know which quadrant all players are located in.
		if(PlayerLocator)
		{
			PlayerLocator.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 10);
			PlayerLocator.transform.position = new Vector3(PlayerLocator.transform.position.x, PlayerLocator.transform.position.y, 0);
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

	public void SetPlayerName(string name)
	{
		Name = name;
	}

	public void OnLevelWasLoaded(int level)
	{
		// TerrainMesh must be set when the level is started because the HexMesh object is not created
		// until the level loads. All levels MUST begin with a defined prefix for this to work properly
		if(Application.loadedLevelName.StartsWith("Level"))
		{
			TerrainMesh = GameObject.FindGameObjectWithTag("Terrain").GetComponent<HexMesh>() as HexMesh;
			// Instantiate a player locator point that is used for the allies' Radar
			PlayerLocator = SessionManager.Instance.InstantiateObject("PlayerLocator",  new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
			PlayerLocator.name = Name;
		}
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
