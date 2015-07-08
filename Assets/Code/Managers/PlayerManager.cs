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
	private TowerData PlacementTowerData;
	private HexMesh TerrainMesh;
	
	private Dictionary<string, string> LevelProgress;
	
	private int PlayerColorIndex;
	
	// Componentes
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
							// Tell all other players that an Enemy has spawned (SpawnEnemyAcrossNetwork is currently in GameManager.cs)
							ObjPhotonView.RPC ("SpawnTowerAcrossNetwork", PhotonTargets.All, PlacementTowerData.DisplayName, hit.point, SessionManager.Instance.AllocateNewViewID());
							
							// TO DO: Instantiate tower how enemies are instantiated - manually
							// Tell all players to instantiate a tower
							//SessionManager.Instance.InstantiateObject("Towers/" + PlacementTowerData.PrefabName, hit.point, rotation);
							
							LastTowerPlacementTime = Time.time;
							
							// Charge the player for building the tower
							this.Money -= PlacementTowerData.InstallCost;
						}
					}
					else
					{
						// This currently does not work. My intent is to display this message alongside the place where the player clicks
						NotificationManager.Instance.DisplayNotification(new NotificationData("", "Insufficient Funds", "InsufficientFunds", 0, Input.mousePosition));
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
	
	[PunRPC]
	private void SpawnTowerAcrossNetwork(string displayName, Vector3 position, int viewID, PhotonMessageInfo info)
	{
		// Create a "Look" quaternion that considers the Z axis to be "up" and that faces away from the base
		var rotation = Quaternion.LookRotation(position, new Vector3(0.0f, 0.0f, -1.0f));
		
		// Instantiate a new Enemy
		GameObject newTower = Instantiate(Resources.Load("Towers/" + GameDataManager.Instance.FindTowerPrefabByDisplayName(displayName)), position, rotation) as GameObject;
		// Add a PhotonView to the Tower
		newTower.AddComponent<PhotonView>();
		// Set Tower's PhotonView to match the Master Client's PhotonView ID for this GameObject (these IDs must match for networking to work)
		newTower.GetComponent<PhotonView>().viewID = viewID;
		// The Prefab doesn't contain the correct default data. Set the Tower's default data now
		newTower.GetComponent<Tower>().SetTowerData(GameDataManager.Instance.FindTowerDataByDisplayName(displayName), PlayerColors.colors[(int)info.sender.customProperties["PlayerColorIndex"]]);
	}
	
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
			Debug.Log("[PlayerManager] " + message);
	}
	
	protected void LogError(string message)
	{
		Debug.LogError("[PlayerManager] " + message);
	}
	#endregion
}