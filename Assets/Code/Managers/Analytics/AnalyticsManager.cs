using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

/// <summary>
/// A collection of analytics for the player, level, and assets
/// Owner: John Fitzgerald
/// </summary>
public class AnalyticsManager : MonoBehaviour
{
    private static AnalyticsManager instance;
    public bool ShowDebugLogs = true;

    // Send all the level stats to the Unity Analaytics server.
    // *************************************************
    // http://docs.unity3d.com/Manual/UnityAnalyticsCustomAttributes51.html
    // *************************************************
    //                      RULES
    // Limit 10 paramters per "CustomEvent",
    // Limit 500 characters for the dictionary content,
    // Limit 100 "CustomEvent"s per hour (per user),
    // Only strings and booleans are categorizable.
    // *************************************************

    /// <summary>
    ///         GENERAL RULES:
    /// 1) Only send CustomEvents at the end of a match.
    /// 2) Explanation of shorthand
    ///     a) (MASTER): sent by master client
    ///     b) (ALL): sent by all clients
    ///
    ///
    ///         TASK AT HAND:
    /// 1) For sending global (MASTER) data: only send data from master client.
    /// 2) Compute averages on the user side to avoid sending so many events to the server.
    /// 3) Can I categorize user data by their PlayerName?
    /// </summary>

    //[PLAYER]
    public PhotonView ObjPhotonView { get; private set; }   //
    public string PlayerName { get; private set; }          // Name derived from (Player.Instance.Name) (ALL)
    public float IndividualPlayerMoney { get; private set; }// Player's total money accumulated (ALL)
    public float AllPlayersMoney { get; private set; }      // All player's total money accumulated (MASTER)
    public float AvgPlayerMoney { get; private set; }       // Average of all player's income (MASTER)
    public bool IsMasterClient { get; private set; }        // Indicate if the player is the master client (they will send the global data) (MASTER)

    //[LEVEL'S AVAILABLE TOWERS AND ENEMIES]
    public List<Analytics_TrackedAssets> Available_Towers { get; private set; }   // Track the towers available in level
    public List<Analytics_TrackedAssets> Available_Enemies { get; private set; } // Track the enemies available in level
    
    //[LEVEL]
    public LevelData CurrentLevelData { get; private set; }  // Temp storage for level data
    public int LastLevelReached { get; private set; }        // Last level reached. (ALL)
    public bool IsEndGameVictory { get; private set; }       // If the last level was a win or loss (ALL)
    public int PlayerCount { get; private set; }             // Playercount at the beginning of a level. (ALL & MASTER)
    public bool PlayerCountChanged { get; private set; }     // Indicates if the player count has changed since the beginning of a level (ALL & MASTER)
    public float GameLength { get; private set; }            // Length of time the user plays the level (from game start to loss/win) (MASTER)
    public int LevelID { get; private set; }                 // ID number of level (MASTER)
    public int LevelScore { get; private set; }              // Player's level score

    //[DAMAGE FOR ALL ASSETS BY LEVEL]
    public float TotalBallisticTaken_Tower { get; private set; }    // Accumulated in a single level (ALL & MASTER)
    public float TotalThraceiumTaken_Tower { get; private set; }    // Accumulated in a single level (ALL & MASTER)
    public float TotalBallisticTaken_Enemy { get; private set; }    // Accumulated in a single level (ALL & MASTER)
    public float TotalThraceiumTaken_Enemy { get; private set; }    // Accumulated in a single level (ALL & MASTER)

    //[TOWER AND ENEMY STATS]
    //[TOWERS]
    public int TotalBuilt_Tower { get; private set; }           // 
    public int TotalDead_Tower { get; private set; }            // 
    public float AvgLifeSpanDead_Tower { get; private set; }    // 
    public float AvgDPS_Tower { get; private set; }             // 


    //Avg DPS for each tower type (ALL & MASTER)
    
    
    //Avg distance from center for each tower type (ALL & MASTER)
    //Avg distance from other tower (ALL & MASTER)

    //[ENEMIES]
    public int TotalSpawn_Enemy { get; private set; }           // 
    public int TotalDead_Enemy { get; private set; }            // 
    public float AvgLifeSpanDead_Enemy { get; private set; }    // 
    public float AvgDPS_Enemy { get; private set; }             // 
    //Avg DPS for each enemy type (ALL & MASTER)
    //Find the amount of damage for each enemy to tower (ALL & MASTER)

    //[MISCELLANEOUS]
    public int StolenThraceium { get; private set; }            // Amount of thraceium stolen by players
    public int NumberOfBarriers { get; private set; }           // Number of barriers placed in the level

    #region INSTANCE (SINGLETON)

    /// <summary>
    /// Singleton - There can only be one
    /// </summary>
    /// <value>The instance.</value>
    public static AnalyticsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<AnalyticsManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    #endregion INSTANCE (SINGLETON)

    #region PUBLIC FUNCTIONS

    /// <summary>
    /// Called at the beginning of each level within GameManager
    /// </summary>
    public void InitializePlayerAnalytics()
    {
        CurrentLevelData = GameManager.Instance.CurrentLevelData;
        if (LastLevelReached == null)
            LastLevelReached = 0;
        ObjPhotonView = PhotonView.Get(this);
        PlayerName = PlayerManager.Instance.Username;
        IndividualPlayerMoney = CurrentLevelData.StartingMoney;
        AllPlayersMoney = 0;
        AvgPlayerMoney = 0;
        PlayerCountChanged = false;
        PlayerCount = SessionManager.Instance.GetRoomPlayerCount();
        
        Available_Towers = new List<Analytics_TrackedAssets>();
        Available_Enemies = new List<Analytics_TrackedAssets>();

        // Create list of available towers in the level
        foreach (TowerData tower in GameDataManager.Instance.TowerDataManager.DataList)
        {
            // Have to manually add Mining Facility as it uses the Tower class, 
            // is the only one of its kind, and is not included in the Level data
            Available_Towers.Add(new Analytics_TrackedAssets("Mining Facility"));

            if (CurrentLevelData.AvailableTowers.Contains(tower.DisplayName))
                Available_Towers.Add(new Analytics_TrackedAssets(tower.DisplayName));
        }
        // Create list of available enemies in the level
        foreach (EnemyData enemy in GameDataManager.Instance.EnemyDataManager.DataList)
        {
            if (CurrentLevelData.AvailableEnemies.Contains(enemy.DisplayName))
                Available_Enemies.Add(new Analytics_TrackedAssets(enemy.DisplayName));
        }

        // Determine Master Client
        IsMasterClient = SessionManager.Instance.GetPlayerInfo().isMasterClient ? true : false;

        // Asset Analytics
        TotalBuilt_Tower = 0;
        TotalSpawn_Enemy = 0;
        TotalDead_Tower = 0;
        TotalDead_Enemy = 0;
        AvgLifeSpanDead_Tower = 0;
        AvgLifeSpanDead_Enemy = 0;
        AvgDPS_Tower = 0;
        AvgDPS_Enemy = 0;
    }

    /// <summary>
    /// Public call to save, send, and reset all analytics
    /// </summary>
    public void PerformAnalyticsProcess()
    {
        SaveAnalytics();
        SendAnalytics();
        ResetAnalytics();
    }

    /// <summary>
    /// Sets the user as the master client
    /// </summary>
    public void SetIsMaster(bool isMaster)
    {
        IsMasterClient = isMaster;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetPlayerCountChanged(bool playerCountChanged)
    {
        PlayerCountChanged = playerCountChanged;
    }

    /// <summary>
    /// Find Tower category by its display name
    /// </summary>
    public Analytics_TrackedAssets FindTowerByDisplayName(string displayName)
    {
        return Available_Towers.Find(x => x.DisplayName == displayName);
    }

    /// <summary>
    /// Find Enemy category by its display name
    /// </summary>
    public Analytics_TrackedAssets FindEnemyByDisplayName(string displayName)
    {
        return Available_Enemies.Find(x => x.DisplayName == displayName);
    }

    /// <summary>
    /// Save Player analytics
    /// </summary>
    public void SavePlayerAnalytics()
    {
        IndividualPlayerMoney = PlayerManager.Instance.TotalMoney;
        if (!IsMasterClient)
            ObjPhotonView.RPC("SavePlayers_MoneyMasterClient", PhotonTargets.MasterClient, IndividualPlayerMoney);
        else
            AllPlayersMoney += IndividualPlayerMoney;

    }

    /// <summary>
    /// Save Level analytics
    /// </summary>
    public void SaveSessionAnalytics()
    {
        IsEndGameVictory = GameManager.Instance.Victory;
        GameLength = Time.time - GameManager.Instance.LevelStartTime;
        LevelID = CurrentLevelData.LevelID;
        if (CurrentLevelData.LevelID > LastLevelReached && IsEndGameVictory == true)
            LastLevelReached = CurrentLevelData.LevelID;
        LevelScore = PlayerManager.Instance.Score;
        PlayerCountChanged = PlayerCount != SessionManager.Instance.GetRoomPlayerCount() ? true : false;
    }

    /// <summary>
    /// Save Asset analytics (enemy and tower stats)
    /// </summary>
    public void SaveAssetAnalytics()
    {
        // Send Tower spawn&death informtion
        foreach (Analytics_TrackedAssets towerAssets in Available_Towers)
        {
            // Collect group information
            if (towerAssets.DisplayName != "Mining Facility")
            {
                TotalBuilt_Tower += towerAssets.GetNumberCreated();
                TotalDead_Tower += towerAssets.GetNumberDead();
                AvgLifeSpanDead_Tower += towerAssets.GetTotalLifeSpanOfDead();
                AvgDPS_Tower += towerAssets.GetTotalDPS();
            }

            //// Collect individual Tower analytics
            //foreach (Analytics_Asset tower in towerAssets.Assets)
            //{
            //}
        }

        // Send Enemy death information
        foreach (Analytics_TrackedAssets enemyAssets in Available_Enemies)
        {
            // Collect group information
            TotalSpawn_Enemy += enemyAssets.GetNumberCreated();
            TotalDead_Enemy += enemyAssets.GetNumberDead();
            AvgLifeSpanDead_Enemy += enemyAssets.GetTotalLifeSpanOfDead();
            AvgDPS_Enemy += enemyAssets.GetTotalDPS();

            //// Collect individual Enemy analytics
            //foreach (Analytics_Asset enemy in enemyAssets.Assets)
            //{
            //}
        }

            AvgLifeSpanDead_Tower /= TotalDead_Tower;
            AvgLifeSpanDead_Enemy /= TotalDead_Enemy;
            AvgDPS_Tower /= TotalBuilt_Tower;
            AvgDPS_Tower /= TotalSpawn_Enemy;
    }

    /// <summary>
    /// Public call to send Player analytics.
    /// </summary>
    public void SendPlayerAnalytics()
    {
        //PlayerAnalytics_Send();
    }

    public void SendAssetAnalytics()
    {
        //AssetAnalytics_Send();
        HeatMapAnalytics_Send();
    }

    /// <summary>
    /// Public call to send session/level analytics.
    /// </summary>
    public void SendSessionAnalytics()
    {
        //LevelAnalytics_Send();
        //MiscellaneousAnalytics_Send();
        //InitialAnalytics_Send();
    }
    #endregion PUBLIC FUNCTIONS

    #region RESET ANALYTICS

    /// <summary>
    /// Resets all variables (used for a soft restart)
    /// </summary>
    public void ResetAllAnalytics()
    {
        ResetPlayerAnalytics();
        ResetSessionAnalytics();
        ResetAssetAnalytics();
    }

    /// <summary>
    /// Resets player analytics
    /// </summary>
    public void ResetPlayerAnalytics()
    {
        IndividualPlayerMoney = 0;
        IsMasterClient = false;
    }

    /// <summary>
    /// Resets level analytics
    /// </summary>
    public void ResetSessionAnalytics()
    {
        TotalBallisticTaken_Tower = 0;
        TotalThraceiumTaken_Tower = 0;
        TotalBallisticTaken_Enemy = 0;
        TotalThraceiumTaken_Enemy = 0;
        GameLength = 0;
        PlayerCount = 0;
        PlayerCountChanged = false;
        //LastLevelReached = 1;

        Available_Towers = new List<Analytics_TrackedAssets>();
        Available_Enemies = new List<Analytics_TrackedAssets>();
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetAssetAnalytics()
    {

    }
    #endregion RESET ANALYTICS

    #region RPC Calls
    /// <summary>
    /// 
    /// </summary>
    [PunRPC]
    private void SavePlayers_MoneyMasterClient(float playerMoney)
    {
        AllPlayersMoney += playerMoney;
    }
    #endregion RPC Calls

    #region PRIVATE SAVE/SEND/RESET FUNCTIONS
    /// <summary>
    /// Saves all relevant analytics
    /// </summary>
    private void SaveAnalytics()
    {
        // All players save their personal analytics.
        SavePlayerAnalytics();

        // If master client, save level anyltics.
        if (IsMasterClient)
        {
            SaveSessionAnalytics();
            SaveAssetAnalytics();
        }
    }

    /// <summary>
    /// Sends all relevant analytics
    /// </summary>
    private void SendAnalytics()
    {
        // Each player sends their personal analytics.
        SendPlayerAnalytics();

        // If the user is the master client and the number of players haven't changed, send and reset the level anyltics.
        if (IsMasterClient)
        {
            SendSessionAnalytics();
            SendAssetAnalytics();
        }
    }

    /// <summary>
    /// Resets all relevant analytics
    /// </summary>
    private void ResetAnalytics()
    {
        ResetPlayerAnalytics();
        ResetSessionAnalytics();
        ResetAssetAnalytics();
    }

    /// <summary>
    /// [PLAYER] Send player's analytics
    /// </summary>
    private void PlayerAnalytics_Send()
    {
        Analytics.CustomEvent("PlayerStats", new Dictionary<string, object>
        {
            {"PlayerName", PlayerManager.Instance.Username},
            {"PlayerIncome", IndividualPlayerMoney}
        });
    }

    /// <summary>
    /// [LEVEL] Send level analytics
    /// </summary>
    private void LevelAnalytics_Send()
    {
        Analytics.CustomEvent("LastLevelReached_Event", new Dictionary<string, object>
        {
            {"LastLevelReached", LastLevelReached},
            {"CurrentPlayerCount", PlayerCount},
            {"GameLength", GameLength},
            {"LevelName", GameManager.Instance.CurrentLevelData.SceneName},
            {"LevelID", LevelID}
            //{"PlayerName", PlayerManager.Instance.Username}
        });
    }

    /// <summary>
    /// [ASSETS] Send assets' (tower & enemy) analytics
    /// </summary>
    private void AssetAnalytics_Send()
    {
        //TowerAnalytics_Send();
        //EnemyAnalytics_Send();
        //DamageAnalytics_Send();
    }

    // Combine with LevelAnalytics?
    /// <summary>
    /// [DAMAGE] Level's total ballistic and thraceium damage dealt by player.
    /// </summary>
    private void DamageAnalytics_Send()
    {
        Analytics.CustomEvent("TotalLevelDamage_Event", new Dictionary<string, object>
        {
            {"TowerBallisticTaken", TotalBallisticTaken_Tower},
            {"TowerThraceiumTaken", TotalThraceiumTaken_Tower},
            
            {"EnemyBallisticTaken", TotalBallisticTaken_Enemy},
            {"EnemyThraceiumTaken", TotalThraceiumTaken_Enemy},
            
            {"LevelName", GameManager.Instance.CurrentLevelData.SceneName},
            {"LevelID", LevelID}
        });
    }

    /// <summary>
    /// [HEATMAP] Location of asset (enemy and tower) activty on the map
    /// </summary>
    private void HeatMapAnalytics_Send()
    {
        // Send Tower spawn&death informtion
        foreach (Analytics_TrackedAssets assets in Available_Towers)
        {
            foreach (Analytics_Asset tower in assets.Assets)
                HeatMapAnalyticsSingleAsset_Send(tower);
        }

        // Send Enemy death information
        foreach (Analytics_TrackedAssets assets in Available_Enemies)
        {
            foreach (Analytics_Asset enemy in assets.Assets)
                HeatMapAnalyticsSingleAsset_Send(enemy);
        }

        Log("HeatMap analytics for tower&enemy deaths and tower spawns have been sent to Unity's server.");
    }

    /// <summary>
    /// [HEATMAP] Send a single asset's information to Unity's Heatmap server
    /// </summary>
    private void HeatMapAnalyticsSingleAsset_Send(Analytics_Asset asset)
    {
        string eventName = asset.AssetSupertype;
        string eventDetails = "|Lvl:" + CurrentLevelData.LevelID + "|Plyrs:" + PlayerCount;
        Vector3 assetLocationData;
        float assetTimeData;

        // Server will only allow Dictionaries with: strings, ints, and bools
        Dictionary<string, object> assetDetails = new Dictionary<string, object> 
        { 
            // Incase we change scene names and level ids in the future, this will allow us to validate the data
            { "assetSubtype", asset.AssetSubtype},                  // e.g. "EMP"
            { "assetLifeSpan", asset.LifeSpan},                     // e.g. 23
            { "sceneDisplayName", CurrentLevelData.DisplayName},    // e.g. "Better with Friends"
            { "dateTimeUTC", DateTime.UtcNow.ToString()}            // e.g. "11/5/2015 2:53:32 AM"
        };

        // e.g. "TowerDeath|Lvl:1|Plyrs:1"
        eventName += asset.IsDead ? "Death" + eventDetails : "Spawn" + eventDetails;

        // Determine if data sent will be about the asset's spawn or death
        assetLocationData = asset.IsDead ? asset.LocationOfDeath : asset.LocationOfSpawn;
        assetTimeData = asset.IsDead ? asset.TimeOfDeathSinceLoad : asset.TimeOfSpawnSinceLoad;

        // Sends an individual asset's death/spawn data to Unity's HeatMap server 
        UnityAnalyticsHeatmap.HeatmapEvent.Send(eventName, assetLocationData, assetTimeData, assetDetails);

        //LogError(eventName + "|" + DateTime.UtcNow.ToString());
        //LogError("SpawnTime: " + asset.TimeOfSpawnSinceLoad);
        //LogError("DeathTime: " + asset.TimeOfDeathSinceLoad);
    }

    /// <summary>
    /// Set optional stats for Analytics.
    /// </summary>
    private void InitialAnalytics_Send()
    {
        //Analytics.SetUserBirthYear(1986);
        //Analytics.SetUserGender(0);
        //Analytics.SetUserId("ID Number");
    }

    // [TOWERS]
    private void TowerAnalytics_Send()
    {
        Analytics.CustomEvent("TowerData_Event", new Dictionary<string, object>
        {
            {"TotalBuilt_Tower", TotalBuilt_Tower},
            {"LifeSpan_Tower", AvgLifeSpanDead_Tower},
        });
    }

    // [ENEMIES]
    private void EnemyAnalytics_Send()
    {
        Analytics.CustomEvent("EnemyData_Event", new Dictionary<string, object>
        {
            {"TotalSpawn_Enemy", TotalSpawn_Enemy},
            {"LifeSpan_Enemy", AvgLifeSpanDead_Enemy},
        });
    }

    // [MISCELLANEOUS]
    private void MiscellaneousAnalytics_Send()
    {
        Analytics.CustomEvent("Miscellaneous_Event", new Dictionary<string, object>
        {
            {"StolenThraceium", StolenThraceium},
            {"NumberOfBarriers", NumberOfBarriers}
        });
    }
    #endregion SEND ANALYTICS

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[AnalyticsManager] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[AnalyticsManager] " + message);
    }

    #endregion MessageHandling
}