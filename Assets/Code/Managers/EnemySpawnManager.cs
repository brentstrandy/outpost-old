using UnityEngine;
using System;
using System.Collections;

public class EnemySpawnManager : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	public bool FinishedLoadingData { get; private set; }
	public bool FinishedSpawning { get; private set; }

	private static EnemySpawnManager instance;
	private EnemySpawnDataHandler SpawnDataHandler;
	private string LevelName;
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
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<EnemySpawnManager>();
			}
			
			return instance;
		}
	}

	void Awake()
	{
		instance = this;
	}
	#endregion

	public void Start()
	{
        LevelName = Application.loadedLevelName;
		FinishedSpawning = false;
		FinishedLoadingData = false;

		ObjPhotonView = gameObject.GetComponent<PhotonView>();

		// Instantiate data class for storing all Enemy Spawn Data
		SpawnDataHandler = new EnemySpawnDataHandler();
		// Run coroutine to download EnemySpawnData from server
		StartCoroutine(LoadDataFromServer(LevelName));
	}

	/// <summary>
	/// Coroutine method used to load XML data from a server location
	/// </summary>
	public IEnumerator LoadDataFromServer(string levelName)
	{
		WWW www = new WWW("http://www.diademstudios.com/outpostdata/" + levelName + ".xml");
		string myXML;
		
		while(!www.isDone)
		{
			yield return 0;
		}
		
		myXML = www.text;
		
		// Deserialize XML and add each enemy spawn to the lists
		foreach (EnemySpawnData spawnData in XMLParser<EnemySpawnData>.XMLDeserializer_Server(myXML))
		{
			SpawnDataHandler.AddSpawnData(spawnData);
		}

		// Spawning can now begin, waiting for final approval to begin spawning
		FinishedLoadingData = true;
	}

	// Update is called once per frame
	public void Update ()
	{

	}

	/// <summary>
	/// Starts the enemy spawning coroutine
	/// </summary>
	public void StartSpawning()
	{
		StartTime = Time.time;
		// Start a coroutine that spawns enemies whenever necessary (according to data loaded from XML
		StartCoroutine("SpawnEnemies");
	}

	/// <summary>
	/// Stops the enemy spawning coroutine
	/// </summary>
	public void StopSpawning()
	{
		StartTime = -1;
		StopCoroutine("SpawnEnemies");
	}

	/// <summary>
	/// Validate that an enemy can be spawned and if so, spawn it across the network
	/// </summary>
	/// <param name="spawnDetails">Enemy Spawn Data</param>
    private void SpawnEnemy(EnemySpawnData spawnDetails)
    {
		// Only spawn an enemy if you are the Master Client. Otherwise, the Master client will tell all clients to spawn an enemy
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// Only spawn the enemy if there are the appropriate number of co-op players
			if(SessionManager.Instance.GetRoomPlayerCount() >= spawnDetails.PlayerCount)
			{
				// Tell all other players that an Enemy has spawned.
				ObjPhotonView.RPC("SpawnEnemyAcrossNetwork", PhotonTargets.All, spawnDetails.EnemyName, spawnDetails.StartAngle, SessionManager.Instance.AllocateNewViewID());
			}
		}
    }

	/// <summary>
	/// Spawn's Enemy on all client machines.
	/// An PunRPC option is needed in order to set the Enemy's default data AFTER being created
	/// </summary>
	/// <param name="displayName">Display name.</param>
	/// <param name="startAngle">Start angle.</param>
	/// <param name="viewID">View ID.</param>
	[PunRPC]
	private void SpawnEnemyAcrossNetwork(string displayName, int startAngle, int viewID)
	{
		// Instantiate an Enemy
		GameObject newEnemy = Instantiate(Resources.Load("Enemies/" + GameDataManager.Instance.FindEnemyPrefabNameByDisplayName(displayName)), AngleToPosition(startAngle), Quaternion.identity) as GameObject;
		// Add a PhotonView to the Enemy
		newEnemy.AddComponent<PhotonView>();
		// Set Enemy's PhotonView to match the Master Client's PhotonView ID
		newEnemy.GetComponent<PhotonView>().viewID = viewID;
		// The Prefab doesn't contain the correct default data. Set the Enemy's default data
		newEnemy.GetComponent<Enemy>().SetEnemyData(GameDataManager.Instance.FindEnemyDataByDisplayName(displayName));
	}

	/// <summary>
	/// Calculates the starting position of an enemy based on a given angle
	/// </summary>
	/// <returns>The to position.</returns>
	/// <param name="angle">Angle.</param>
	private Vector3 AngleToPosition(int angle)
	{
        float radians = (Mathf.PI / 180) * angle; // Mathf.Deg2Rad;
        
		// Note from J.S. 2015-03-29: Shouldn't the sin and cos be reversed here? I think that would put 0 degrees to the north, which seems to be typical.
		return new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0) * 25;
	}

	/// <summary>
	/// Coroutine that handles the spawning of all enemies.
	/// </summary>
	/// <returns>Nothing</returns>
	private IEnumerator SpawnEnemies()
	{
		// Loop until there are no enemies left to spawn
		while(!SpawnDataHandler.SpawnListEmpty())
		{
			// Check to see if the next enemy's spawn time has passed. If so, spawn the enemy
			if(SpawnDataHandler.GetNextStartTime() <= (Time.time - this.StartTime))
				SpawnEnemy(SpawnDataHandler.SpawnNext());

			// TO DO: Tell the coroutine to run when the next available enemy is ready
			// SpawnActionHandler.GetNextStartTime() - Time.time
			yield return 0;
		}

		// Spawning is complete
		FinishedSpawning = true;
	}
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[EnemySpawnManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[EnemySpawnManager] " + message);
	}
	#endregion
}
