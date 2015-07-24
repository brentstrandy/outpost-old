using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;

/// <summary>
/// Player is a Singleton used for the entirety of the game session.
/// It is created when the user starts a game session.
/// </summary>
public class PlayerManager : MonoBehaviour
{
	private static PlayerManager instance;
	
	public bool ShowDebugLogs = true;
	public float Money { get; private set; }
	public Quadrant CurrentQuadrant;
	
	private GameObject PlayerLocator;
	public string Name { get; private set; }
	private LoadOut GameLoadOut;
	private double LastTowerPlacementTime;

	// Tower Placement
	private TowerData PlacementTowerData;
	public GameObject PlacementTowerPrefab;

	// Tower Selection
	public HexCoord SelectedTowerCoord { get; private set; }
	
	private Dictionary<string, string> LevelProgress;
	
	private int PlayerColorIndex;
	
	// Components
	PhotonView ObjPhotonView;
	
	public void Start()
	{
		Money = 0.0f;
		PlacementTowerData = null;
		LastTowerPlacementTime = Time.time;
		
		LevelProgress = new Dictionary<string, string>();
		
		ObjPhotonView = PhotonView.Get(this);
		
		// Loop through all known levels and load the player's progress from PlayerPrefs
		foreach(LevelData levelData in GameDataManager.Instance.LevelDataManager.DataList)
		{
			// If the level progress has been saved, retrieve it
			if(PlayerPrefs.HasKey(levelData.DisplayName))
				LevelProgress.Add(levelData.DisplayName, PlayerPrefs.GetString(levelData.DisplayName));
			else
			{
				// If no level progress is saved in PlayerPrefs, add it to the PlayerPrefs and load it into the game as zero progress
				PlayerPrefs.SetString(levelData.DisplayName, "false~0");
				LevelProgress.Add(levelData.DisplayName, "false~0");
			}
		}

		// Save any updates to the PlayerPrefs for safe measure (this action is also done on Application.Quit)
		PlayerPrefs.Save();
	}

	#region INSTANCE (SINGLETON)
	/// <summary>
	/// Singleton - There can be only one
	/// </summary>
	/// <value>The instance.</value>
	public static PlayerManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<PlayerManager>();
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
		// TODO: Consider moving all of this to the InputManager

		// TODO: Do not perform this action if the player has clicked a button, tower, or any other object besides a blank Hex tile
		// Check for player input (mouse click)

		if (GameManager.Instance != null && GameManager.Instance.GameRunning)
		{
			var terrain = GameManager.Instance.TerrainMesh;

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			HexCoord coord;

			// Test to see if the player's click intersected with the Terrain (HexMesh)
			if (terrain.IntersectRay(ray, out hit, out coord) && terrain.InPlacementRange(coord))
			{
				if (Input.GetMouseButtonDown(0))
				{
					bool existing = GameManager.Instance.TowerManager.HasTower(coord);

					// Check to see if a tower has been selected by the player for placement
					if (PlacementTowerData != null)
					{
						// Make sure that there isn't an existing tower on the plot
						if (!existing)
						{
							if (!GameManager.Instance.TerrainMesh.IsImpassable(coord))
							{
								// Ensure towers cannot be repeatedly placed every frame
								if (Time.time - LastTowerPlacementTime > 1)
								{
									// Make sure the player has enough money to place the tower
									if (this.Money >= PlacementTowerData.InstallCost)
									{
										// FIXME: Tenatively place the tower but be prepared to roll back the change if the
										//        master reports a conflict with another player's tower placement at the same
										//        location at the same time
										LastTowerPlacementTime = Time.time;
										
										// Charge the player for building the tower
										this.Money -= PlacementTowerData.InstallCost;
										
										// Calculate the tower placement based on the clicked coordinate
										//var position = terrain.IntersectPosition((Vector3)coord.Position());
										
										// Tell all other players that an Enemy has spawned (SpawnEnemyAcrossNetwork is currently in GameManager.cs)
										ObjPhotonView.RPC("SpawnTowerAcrossNetwork", PhotonTargets.All, PlacementTowerData.DisplayName, coord, SessionManager.Instance.AllocateNewViewID());
									}
									else
									{
										// This currently does not work. My intent is to display this message alongside the place where the player clicks
										NotificationManager.Instance.DisplayNotification(new NotificationData("", "Insufficient Funds", "InsufficientFunds", 0, Input.mousePosition));
									}
								}
							}
						}
					}
					// Check to see if the user clicked an existing tower
					else if (existing)
					{
						// Mouse click on existing tower
						Select(coord);

						// Don't display a highlight when we need to display a selection
						//RemoveHighlight();
					}
					else
					{
						// Mouse click on non-tower location
						RemoveSelection();
					}
				}
				else
				{
					// Mouseover in placement area
					Highlight(coord);
				}
			}
			else
			{
				// Mouseover outside of placement area
				RemoveHighlight();
				if (Input.GetMouseButtonDown(0))
				{
					// Mouse click outside of placement area
					RemoveSelection();
				}
			}
		}
		
		// Always keep the player locator GameObject in front of the camera. The GameObject has a PhotonView attached to it for the RadarManager
		// to know which quadrant all players are located in.
		if (PlayerLocator)
		{
			PlayerLocator.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 10);
			PlayerLocator.transform.position = new Vector3(PlayerLocator.transform.position.x, PlayerLocator.transform.position.y, 0);
		}
	}
	
	#region Player Tower Interface
	[PunRPC]
	private void SpawnTowerAcrossNetwork(string displayName, HexCoord coord, int viewID, PhotonMessageInfo info)
	{
		// TODO: Coordinate tower spawning when two players build a tower in the same position at the same time.
		// TODO: Sanitize displayName if necessary. For example, make sure it matches against a white list. Also, if it contained "../blah", what would be the effect?

		if (GameManager.Instance.TowerManager.HasTower(coord))
		{
			LogError("Tower construction conflict! Potential inconsistent game state.");
		}

		var position = GameManager.Instance.TerrainMesh.IntersectPosition((Vector3)coord.Position());

		// Create a "Look" quaternion that considers the Z axis to be "up" and that faces away from the base
		var rotation = Quaternion.LookRotation(new Vector3(position.x, position.y, 0.0f), new Vector3(0.0f, 0.0f, -1.0f));
		
		// Instantiate a new Enemy
		GameObject newTower = Instantiate(Resources.Load("Towers/" + GameDataManager.Instance.FindTowerPrefabByDisplayName(displayName)), position, rotation) as GameObject;
		// Add a PhotonView to the Tower
		newTower.AddComponent<PhotonView>();
		// Set Tower's PhotonView to match the Master Client's PhotonView ID for this GameObject (these IDs must match for networking to work)
		newTower.GetComponent<PhotonView>().viewID = viewID;
		// The Prefab doesn't contain the correct default data. Set the Tower's default data now
		newTower.GetComponent<Tower>().SetTowerData(GameDataManager.Instance.FindTowerDataByDisplayName(displayName), PlayerColors.colors[(int)info.sender.customProperties["PlayerColorIndex"]]);

		// Deselect the selected tower (force the user to select a new one)
		PlacementTowerData = null;
		// Destroy the shell prefab
		Destroy(PlacementTowerPrefab);
		PlacementTowerPrefab = null;
	}

	private void Highlight(HexCoord coord)
	{
		var terrain = GameManager.Instance.TerrainMesh;
		var overlay = terrain.Overlays[(int)TerrainOverlays.Highlight][PhotonNetwork.player.ID];

		overlay.Update(coord);
		overlay.Color = PlayerColors.colors[PlayerColorIndex];
		overlay.Show();
		
		// Show the selected tower (if applicable)
		SetShellTowerPosition(terrain.IntersectPosition((Vector3)coord.Position()));//coord.Position());
	}
	
	private void RemoveHighlight()
	{
		var overlay = GameManager.Instance.TerrainMesh.Overlays[(int)TerrainOverlays.Highlight][PhotonNetwork.player.ID];
		overlay.Hide();
	}
	
	private void Select(HexCoord coord)
	{
		var overlay = GameManager.Instance.TerrainMesh.Overlays[(int)TerrainOverlays.Selection][PhotonNetwork.player.ID];

		overlay.Update(coord);
		overlay.Color = PlayerColors.colors[PlayerColorIndex];
		overlay.Show();

		// Deselect the old tower
		if (SelectedTowerCoord != coord)
		{
			GameObject previousTower;
			if (GameManager.Instance.TowerManager.TryGetTower(SelectedTowerCoord, out previousTower))
			{
				previousTower.GetComponent<Tower>().OnDeselect();
			}
		}

		// Update state
		SelectedTowerCoord = coord;

		// Select the new tower
		GameObject selectedTower;
		if (GameManager.Instance.TowerManager.TryGetTower(coord, out selectedTower))
		{
			selectedTower.GetComponent<Tower>().OnSelect();
		}

		// Tell all other players that this player has selected a tower
		ObjPhotonView.RPC("SelectTowerAcrossNetwork", PhotonTargets.Others, coord);
	}
	
	[PunRPC]
	private void SelectTowerAcrossNetwork(HexCoord coord, PhotonMessageInfo info)
	{
		Log(info.sender.name + " selects " + coord.ToString());
		var overlay = GameManager.Instance.TerrainMesh.Overlays[(int)TerrainOverlays.Selection][info.sender.ID];

		if (coord == default(HexCoord))
		{
			overlay.Hide();
		}
		else
		{
			overlay.Update(coord);
			overlay.Color = PlayerColors.colors[(int)info.sender.customProperties["PlayerColorIndex"]];
			overlay.Show();
		}
	}
	
	private void RemoveSelection()
	{
		var overlay = GameManager.Instance.TerrainMesh.Overlays[(int)TerrainOverlays.Selection][PhotonNetwork.player.ID];

		overlay.Hide();
		
		// Deselect the old tower
		GameObject selectedTower;
		if (GameManager.Instance.TowerManager.TryGetTower(SelectedTowerCoord, out selectedTower))
		{
			selectedTower.GetComponent<Tower>().OnDeselect();
		}

		// Update state
		SelectedTowerCoord = default(HexCoord);
		
		// Tell all other players that this player has deselected a tower
		ObjPhotonView.RPC("SelectTowerAcrossNetwork", PhotonTargets.Others, default(HexCoord));
	}
	#endregion Player Tower Interface
	
	public bool LevelComplete(string levelDisplayName)
	{
		string details;
		string[] detailsSplit;
		bool playedLevel = false;
		
		// Try to find the level details within the dictionary
		if(LevelProgress.TryGetValue(levelDisplayName, out details))
		{
			detailsSplit = details.Split('~');
			if(detailsSplit[0] == "True")
				playedLevel = true;
		}

		return playedLevel;
	}
	
	public int LevelScore(string levelDisplayName)
	{
		string details;
		string[] detailsSplit;
		int score = 0;
		
		// Try to find the level details within the dictionary
		if(LevelProgress.TryGetValue(levelDisplayName, out details))
		{
			detailsSplit = details.Split('~');
			int.TryParse(detailsSplit[1], out score);
		}
		
		return score;
	}
	
	public void SaveLevelProgress(string levelDisplayName, bool complete, int score)
	{
		// Save progress on the level to the player prefs
		PlayerPrefs.SetString(levelDisplayName, complete.ToString() + "~" + score.ToString());

		// Save progress locally
		if(LevelProgress.ContainsKey(levelDisplayName))
			LevelProgress[levelDisplayName] = complete.ToString() + "~" + score.ToString();
		else
			LevelProgress.Add(levelDisplayName, complete.ToString() + "~" + score.ToString());

		PlayerPrefs.Save();
	}
	
	public void TowerSelectedForPlacement(TowerData towerData)
	{
		PlacementTowerData = towerData;

		// Create a "Look" quaternion that considers the Z axis to be "up" and that faces away from the base
		var rotation = Quaternion.LookRotation(new Vector3(10, 10, 0), new Vector3(0.0f, 0.0f, -1.0f));

		// Remove the old prefab (if applicable)
		if(PlacementTowerPrefab != null)
			Destroy(PlacementTowerPrefab);

		// Load the shell prefab to show
		PlacementTowerPrefab = Instantiate(Resources.Load("Towers/" + towerData.PrefabName + "_Shell"), Vector3.zero, rotation) as GameObject;

		// Set the range based on player prefab
		PlacementTowerPrefab.GetComponentInChildren<SpriteRenderer>().transform.localScale *= towerData.AdjustedRange;
	}

	public void SetShellTowerPosition(Vector3 newPosition)
	{
		// Only place the tower if a tower has been selected
		if(PlacementTowerPrefab != null)
		{
			PlacementTowerPrefab.transform.position = newPosition;
		}
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
	
	public void SetStartingMoney(float amount)
	{
		Money = amount;
	}
	
	public void ResetData()
	{
		GameLoadOut = null;
		Money = 0.0f;
		Destroy(PlayerLocator);
	}
	
	public void SetPlayerName(string name)
	{
		Name = name;
	}
	
	public void OnLevelWasLoaded(int level)
	{
		// All levels MUST begin with a defined prefix for this to work properly
		if(Application.loadedLevelName.StartsWith("Level"))
		{
			// Instantiate a player locator point that is used for the allies' Radar
			PlayerLocator = SessionManager.Instance.InstantiateObject("PlayerLocator",  new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
			PlayerLocator.name = Name;
		}
	}
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[PlayerManager] " + message);
	}
	
	protected void LogError(string message)
	{
		Debug.LogError("[PlayerManager] " + message);
	}
	#endregion
}