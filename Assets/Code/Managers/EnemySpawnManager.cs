using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    public bool ShowDebugLogs = true;

	private static EnemySpawnManager instance;

	public DataManager<EnemySpawnData> EnemySpawnDataManager { get; private set; }

	public bool FinishedLoadingData { get; private set; }
	public bool FinishedSpawning { get; private set; }

    private List<EnemySpawnData> SpawnDataList;
	private GameObject[] SpawnAreas;

    private string EnemySpawnFilename;
    private float StartTime;
    private PhotonView ObjPhotonView;

	#region INSTANCE (SINGLETON)

	/// <summary>
	/// Singleton - There can only be one
	/// </summary>
	/// <value>The instance.</value>
	public static EnemySpawnManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = GameObject.FindObjectOfType<EnemySpawnManager>();
			}

			return instance;
		}
	}

	private void Awake()
	{
		instance = this;
	}

	#endregion INSTANCE (SINGLETON)

	public void Start()
    {
		ObjPhotonView = PhotonView.Get(this);

		SpawnAreas = GameObject.FindGameObjectsWithTag("Enemy Spawn Area").OrderBy(x => x.name).ToArray();

		// Begin gathering Enemy Spawn data that the EnemySpawnManager will eventually use
		EnemySpawnDataManager = new DataManager<EnemySpawnData>();
		EnemySpawnDataManager.OnDataLoadSuccess += OnEnemySpawnDataDownloaded_Event;

		FinishedLoadingData = false;
		FinishedSpawning = false;

		// Grab the Enemy Spawn data from either the web server or local xml file. The EnemySpawnManager will use this
		// data to spawn enemies once it has been loaded into the game
		if (GameDataManager.Instance.DataLocation == "Local")
		{
			EnemySpawnDataManager.LoadDataFromLocal("EnemySpawn/" + GameManager.Instance.CurrentLevelData.EnemySpawnFilename + ".xml");
			OnEnemySpawnDataDownloaded_Event();
		}
		else
			StartCoroutine(EnemySpawnDataManager.LoadDataFromServer("http://www.diademstudios.com/outpostdata/EnemySpawn/" + GameManager.Instance.CurrentLevelData.EnemySpawnFilename + ".xml"));
    }

	public void StartSpawning()
	{
		StartCoroutine(SpawnEnemies());
	}

    /// <summary>
    /// Validate that an enemy can be spawned and if so, spawn it across the network
    /// </summary>
    /// <param name="spawnDetails">Enemy Spawn Data</param>
    private void SpawnEnemy(EnemySpawnData spawnDetails)
    {
        // Only spawn an enemy if you are the Master Client. Otherwise, the Master client will tell all clients to spawn an enemy
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            // Only spawn the enemy if there are the appropriate number of co-op players
            if (SessionManager.Instance.GetRoomPlayerCount() >= spawnDetails.PlayerCount)
            {
                // Asset's unique ViewID
                int viewID = SessionManager.Instance.AllocateNewViewID();

                // Tell all other players that an Enemy has spawned (SpawnEnemyAcrossNetwork is currently in GameManager.cs)
				ObjPhotonView.RPC("SpawnEnemyAcrossNetwork", PhotonTargets.All, spawnDetails.EnemyName, spawnDetails.SpawnArea, spawnDetails.StartAngle, viewID);
            }
        }
    }

    /// <summary>
    /// Coroutine that handles the spawning of all enemies.
    /// </summary>
    /// <returns>Nothing</returns>
    private IEnumerator SpawnEnemies()
    {
        // Remember when spawning started in order to spawn future enemies at the right time
        StartTime = Time.time;

        // Loop until there are no enemies left to spawn
        while (SpawnDataList.Count != 0)
        {
            // Check to see if the next enemy's spawn time has passed. If so, spawn the enemy
            if (GetNextSpawnTime() <= (Time.time - this.StartTime))
                SpawnEnemy(SpawnNext());

            // TO DO: Tell the coroutine to run when the next available enemy is ready
            // SpawnActionHandler.GetNextStartTime() - Time.time
            yield return 0;
        }

        StartTime = -1;

        // Spawning is complete
        FinishedSpawning = true;
    }

    public float GetNextSpawnTime()
    {
        float startTime = -1;

        if (SpawnDataList.Count > 0)
            startTime = SpawnDataList[0].StartTime;

        return startTime;
    }

    private EnemySpawnData SpawnNext()
    {
        EnemySpawnData action = null;

        if (SpawnDataList.Count > 0)
        {
            action = SpawnDataList[0];
            SpawnDataList.RemoveAt(0);
        }

        return action;
    }

    private void SortListByStartTime()
    {
        if (SpawnDataList != null)
            SpawnDataList = SpawnDataList.OrderBy(o => o.StartTime).ToList();
    }

	/// <summary>
	/// Calculates the starting position of an enemy based on a given angle
	/// </summary>
	/// <returns>The to position.</returns>
	/// <param name="angle">Angle.</param>
	public Vector3 AngleToPosition(int angle)
	{
		float radians = (Mathf.PI / 180) * angle; // Mathf.Deg2Rad;

		// Note from J.S. 2015-03-29: Shouldn't the sin and cos be reversed here? I think that would put 0 degrees to the north, which seems to be typical.
		return new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0) * 25;
	}

	#region RPC CALLS
	/// <summary>
	/// Spawn's Enemy on all client machines.
	/// An PunRPC option is needed in order to set the Enemy's default data AFTER being created
	/// </summary>
	/// <param name="displayName">Display name.</param>
	/// <param name="startAngle">Start angle.</param>
	/// <param name="viewID">View ID.</param>
	[PunRPC]
	private void SpawnEnemyAcrossNetwork(string displayName, int spawnArea, int startAngle, int viewID)
	{
		Vector3 position;

		// Look up the enemy's data
		EnemyData data = GameDataManager.Instance.FindEnemyDataByDisplayName(displayName);
		// Calculate the enemy's position
		if(spawnArea == 0 || spawnArea >= SpawnAreas.Count())
			position = GameManager.Instance.TerrainMesh.IntersectPosition(AngleToPosition(startAngle), data.HoverDistance);
		else
			position = SpawnAreas[spawnArea].transform.position;
				
		// Instantiate a new Enemy
		GameObject newEnemy = Instantiate(Resources.Load("Enemies/" + GameDataManager.Instance.FindEnemyPrefabNameByDisplayName(displayName)), position, Quaternion.identity) as GameObject;
		// Add a PhotonView to the Enemy
		newEnemy.AddComponent<PhotonView>();
		// Set Enemy's PhotonView to match the Master Client's PhotonView ID for this GameObject (these IDs must match for networking to work)
		newEnemy.GetComponent<PhotonView>().viewID = viewID;

		// Store the enemy in AnalyticsManager
		if (GameManager.Instance.GameRunning)
		{
			AnalyticsManager.Instance.Assets.AddAsset("Enemy", displayName, viewID, position);
		}

		// The Prefab doesn't contain the correct default data. Set the Enemy's default data now
		newEnemy.GetComponent<Enemy>().SetEnemyData(data);
	}
	#endregion

	#region EVENT
	private void OnEnemySpawnDataDownloaded_Event()
	{
		FinishedLoadingData = true;

		SpawnDataList = EnemySpawnDataManager.DataList;

		SortListByStartTime();
	}
	#endregion

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[EnemySpawnManager] " + message);
    }

    protected void LogError(string message)
    {
        Debug.LogError("[EnemySpawnManager] " + message);
    }

    #endregion MessageHandling
}