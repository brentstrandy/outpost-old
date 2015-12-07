using System;
using System.Reflection;
using System.Collections;
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

    //[NETWORK]
    public PhotonView ObjPhotonView { get; private set; }   // AnalyticsManager's unique PhotonView
    public PhotonPlayer PlayerPhotonView { get; private set; }              // Player's unique PhotonView

    //[PLAYER VALIDATION]
    public bool IsMasterClient { get; private set; }        // Indicate if the player is the master client (they will send the global data)
    public int AccountID { get; private set; }              // The player's unique Account ID as related to their login credentials
    public Dictionary<int, bool> PlayerCheckIns { get; private set; }      // End game check to indicate that all Clients have sent the Master Client their data
    public bool HaveAllUsersCheckedIn {get; private set; }  // Indicates if all bools are true in PlayerCheckIns
    public float UserCheckInTimeout { get; private set; }   // How long to wait for all player's to check-in before the global analytics are finalized

    //[SESSION]
    public LevelData CurrentLevelData { get; private set; }  // Temp storage for level data
    public int LastLevelReached { get; private set; }        // Last level reached. (ALL)
    public bool IsEndGameVictory { get; private set; }       // If the last level was a win or loss (ALL)
    public int PlayerCount { get; private set; }             // Playercount at the beginning of a level. (ALL & MASTER)
    public bool PlayerCountChanged { get; private set; }     // Indicates if the player count has changed since the beginning of a level (ALL & MASTER)
    public float GameLength { get; private set; }            // Length of time the user plays the level (from game start to loss/win) (MASTER)
    public int LevelID { get; private set; }                 // ID number of level (MASTER)

    //[PLAYER]
    public string PlayerName { get; private set; }          // Name derived from (Player.Instance.Name) (ALL)
    public int PhotonViewID { get; private set; }           // The player's unique Photon ID as related to the game session
    public float IndividualPlayerMoney { get; private set; }// Player's total money accumulated (ALL)
    public int PlayerScore { get; private set; }              // Player's level score
    public float AllPlayersMoney { get; private set; }      // All player's total money accumulated (MASTER)
    public float AvgPlayerMoney { get; private set; }       // Average of all player's income (MASTER)
   
    //[ASSETS]
    public Vector3 MiningFacilityLocation { get; private set; }     // Returns the GameManager's location of the Mining Facility
    public Analytics_AssetSuperclass Assets { get; private set; }    // Catalog all asset super types to be tracked in the level
    public string[] AssetSupertypeNames { get; private set; }  
    public float TotalBallisticTaken_Tower { get; private set; }    // Accumulated in a single level (ALL & MASTER)
    public float TotalBallisticTaken_Enemy { get; private set; }    // Accumulated in a single level (ALL & MASTER)
    public float TotalThraceiumTaken_Tower { get; private set; }    // Accumulated in a single level (ALL & MASTER)
    public float TotalThraceiumTaken_Enemy { get; private set; }    // Accumulated in a single level (ALL & MASTER)
    public int TotalBuilt_Tower { get; private set; }               // Sum of all players' towers built
    public int TotalSpawn_Enemy { get; private set; }               // Number of enemies spawned
    public int TotalDead_Tower { get; private set; }                // Sum of all players' towers destroyed
    public int TotalDead_Enemy { get; private set; }                // Sum of all enemies destroyed
    public float TotalAvgLifeSpanDead_Tower { get; private set; }        // Average lifespan of all players' towers destroyed
    public float TotalAvgLifeSpanDead_Enemy { get; private set; }        // Average lifespan of all enemies destroyed
    public float TotalAvgDPS_Tower { get; private set; }                 // Average DPS of all players' towers
    public float TotalAvgDPS_Enemy { get; private set; }                 // Average DPS of all enemies

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
    public void InitializePlayerAnalytics(string [] assetSuperTypes)
    {
        // Network Analytics
        ObjPhotonView = PhotonView.Get(this);
        PlayerPhotonView = SessionManager.Instance.GetPlayerInfo();

        // Play Validation Analytics
        IsMasterClient = SessionManager.Instance.GetPlayerInfo().isMasterClient ? true : false;
        PlayerCheckIns = new Dictionary<int, bool>();
        AccountID = PlayerManager.Instance.AccountID;
        ObjPhotonView.RPC("SendAccountID_All", PhotonTargets.All, AccountID);
        HaveAllUsersCheckedIn = false;
        UserCheckInTimeout = 5f;

        // Session Analytics
        CurrentLevelData = GameManager.Instance.CurrentLevelData;
        IsEndGameVictory = false;
        PlayerCount = SessionManager.Instance.GetRoomPlayerCount();
        PlayerCountChanged = false;
        GameLength = 0;
        LevelID = CurrentLevelData.LevelID;

        // Player Analytics
        PlayerName = PlayerManager.Instance.Username;
        Analytics.SetUserId(PlayerName);    // (12/6/15) At a later date Unity Analytics will allow data aggregated from UserIds.
        PhotonViewID = PlayerPhotonView.ID;
        IndividualPlayerMoney = CurrentLevelData.StartingMoney;
        PlayerScore = 0;
        AllPlayersMoney = 0;
        AvgPlayerMoney = 0;

        // Asset Analytics
        AssetSupertypeNames = assetSuperTypes;
        InitializeAssets();
        TotalBallisticTaken_Tower = 0;
        TotalBallisticTaken_Enemy = 0;
        TotalThraceiumTaken_Tower = 0;
        TotalThraceiumTaken_Enemy = 0;
        TotalBuilt_Tower = 0;
        TotalSpawn_Enemy = 0;
        TotalDead_Tower = 0;
        TotalDead_Enemy = 0;
        TotalAvgLifeSpanDead_Tower = 0;
        TotalAvgLifeSpanDead_Enemy = 0;
        TotalAvgDPS_Tower = 0;
        TotalAvgDPS_Enemy = 0;

        // Miscellaneous Analytics
        StolenThraceium = 0;
        NumberOfBarriers = 0;
    }

    /// <summary>
    /// Initialize the Superclass with Supertypes and Subtypes for Asset sorting
    /// </summary>
    private void InitializeAssets()
    {
        InitializeSupertypesList();
        InitializeSubtypesList();
    }

    /// <summary>
    /// Initialize the super type assets to individual Lists.
    /// </summary>
    private void InitializeSupertypesList()
    {
        Assets = new Analytics_AssetSuperclass(AssetSupertypeNames);
    }

    /// <summary>
    /// Initialize the sub type assets to individual Lists.
    /// </summary>
    private void InitializeSubtypesList()
    {
        var towers = Assets.FindSupertypeByName("Tower");
        var enemies = Assets.FindSupertypeByName("Enemy");
        
        // Must add Mining Facility manually (it derives from Tower but isn't listed as an available Tower)
        towers.AddAssetSubtype("Mining Facility");

        // Create List of available towers in the level based on the xml file
        foreach (TowerData tower in GameDataManager.Instance.TowerDataManager.DataList)
        {
            if (CurrentLevelData.AvailableTowers.Contains(tower.DisplayName))
                towers.AddAssetSubtype(tower.DisplayName);
        }

        // Create List of available enemies in the level based on the xml file
        foreach (EnemyData enemy in GameDataManager.Instance.EnemyDataManager.DataList)
        {
            if (CurrentLevelData.AvailableEnemies.Contains(enemy.DisplayName))
                enemies.AddAssetSubtype(enemy.DisplayName);
        }
    }

    /// <summary>
    /// Sets the Mining Facility location
    /// </summary>
    public void SetMiningFacilityLocation(Vector3 miningFacilityLocation)
    {
        MiningFacilityLocation = miningFacilityLocation;
    }

    /// <summary>
    /// Sets the user as the master client
    /// </summary>
    public void SetIsMaster(bool isMaster)
    {
        IsMasterClient = isMaster;
    }

    /// <summary>
    /// Determine if the player count changed since the game started.
    /// </summary>
    public void SetPlayerCountChanged()
    {
        PlayerCountChanged = PlayerCount != SessionManager.Instance.GetRoomPlayerCount() ? true : false;
    }

    /// <summary>
    /// Outputs information to the Unity editor for use in verifying Analytics tracking.
    /// </summary>
    private void DisplayUserCheckInData()
    {
        LogError("Number of users: " + PlayerCount
                 + "\nPlayer count change: " + PlayerCountChanged.ToString());
        LogError("All player money: " + AllPlayersMoney
                 + "\nUser AccountIDs: ");
        foreach (KeyValuePair<int, bool> pair in PlayerCheckIns)
        {
            LogError(pair.Key.ToString() + " ");
        }
    }

    /// <summary>
    /// Public call to save, send, and reset all analytics.
    /// </summary>
    public void PerformAnalyticsProcess()
    {
        AllAnalytics_Save();
        StartCoroutine(AllAnalytics_SendAndReset());
    }

    /// <summary>
    /// Save Player analytics.
    /// </summary>
    public void SavePlayerAnalytics()
    {
        IndividualPlayerMoney = PlayerManager.Instance.TotalMoney;
        PlayerScore = PlayerManager.Instance.Score;
        SetPlayerCountChanged();

        if (!IsMasterClient)
        {
            ObjPhotonView.RPC("SavePlayersMoney_MasterClient", PhotonTargets.MasterClient, IndividualPlayerMoney);
            ObjPhotonView.RPC("SendCheckIn_MasterClient", PhotonTargets.MasterClient, AccountID);
        }
        else
        {
            SavePlayersMoney_MasterClient(IndividualPlayerMoney);
            SendCheckIn_MasterClient(AccountID);
        }
    }

    /// <summary>
    /// Save Session analytics (works in conjunction with FinalizeSaveSessionAnalytics()).
    /// </summary>
    public void SaveSessionAnalytics()
    {
        IsEndGameVictory = GameManager.Instance.Victory;
        GameLength = Time.time - GameManager.Instance.LevelStartTime;

        // Saves last level reached as the current level if the game was a victory.
        if (CurrentLevelData.LevelID > LastLevelReached && IsEndGameVictory == true)
            LastLevelReached = CurrentLevelData.LevelID;
    }

    /// <summary>
    /// Save Asset analytics (enemy and tower stats)
    /// </summary>
    public void SaveAssetAnalytics()
    {
        foreach (Analytics_AssetSupertype supertype in Assets.AssetSupertypes)
        {
            // Analytics for Towers
            if (supertype.SupertypeName == "Tower")
            {
                foreach (Analytics_AssetSubtype subtype in supertype.AssetSubtypes)
                {
                    if (subtype.SubtypeName != "Mining Facility")
                    {
                        TotalBuilt_Tower += subtype.GetNumberCreated();
                        TotalDead_Tower += subtype.GetNumberDead();
                        TotalAvgLifeSpanDead_Tower += subtype.GetTotalLifeSpanDead();
                        TotalAvgDPS_Tower += subtype.GetTotalDPS();

                        // Collect individual Tower analytics
                        //foreach (Analytics_Asset tower in subtype.Assets)
                        //{
                        //}
                    }
                    // Analytics for Mining Facility
                    else
                    {

                    }
                }
            }
                // Analytics for Enemies
            else if (supertype.SupertypeName == "Enemy")
            {
                foreach (Analytics_AssetSubtype subtype in supertype.AssetSubtypes)
                {
                    TotalSpawn_Enemy += subtype.GetNumberCreated();
                    TotalDead_Enemy += subtype.GetNumberDead();
                    TotalAvgLifeSpanDead_Enemy += subtype.GetTotalLifeSpanDead();
                    TotalAvgDPS_Enemy += subtype.GetTotalDPS();

                    // Collect individual Enemy analytics
                    //foreach (Analytics_Asset enemy in subtype.Assets)
                    //{
                    //}
                }
            }
        }

            TotalAvgLifeSpanDead_Tower /= TotalDead_Tower;
            TotalAvgLifeSpanDead_Enemy /= TotalDead_Enemy;
            
            TotalAvgDPS_Tower /= TotalBuilt_Tower;
            TotalAvgDPS_Enemy /= TotalSpawn_Enemy;
    }

    /// <summary>
    /// Public call to send Player analytics.
    /// </summary>
    public void SendPlayerAnalytics()
    {
        //PlayerAnalytics_Send();
    }

    /// <summary>
    /// Public call to send session/level analytics.
    /// </summary>
    public void SendSessionAnalytics()
    {
        //AllLevelAnalytics_Send();
        //AllMiscellaneousAnalytics_Send();
        //AllInitialAnalytics_Send();
    }

    public void SendAssetAnalytics()
    {
        AssetAnalytics_Send();
        HeatMapAnalytics_Send();
    }
    #endregion PUBLIC FUNCTIONS

    #region RPC Calls
    /// <summary>
    /// Adds the player's ViewID to the PlayerCheckIns.
    /// </summary>
    [PunRPC]
    private void SendAccountID_All(int accountID)
    {
        PlayerCheckIns.Add(accountID, false);
    }

    /// <summary>
    /// Indicates the player has sent their analytics to the MasterCLient.
    /// </summary>
    [PunRPC]
    private void SendCheckIn_MasterClient(int accountID)
    {
        PlayerCheckIns[accountID] = true;
        LogError("#" + accountID + ") Checked in @: " + Time.time);
    }

    /// <summary>
    /// Adds the player's money to all players' money.
    /// </summary>
    [PunRPC]
    private void SavePlayersMoney_MasterClient(float playerMoney)
    {
        AllPlayersMoney += playerMoney;
    }
    #endregion RPC Calls

    #region PRIVATE SAVE/RESET/SEND FUNCTIONS
    /// <summary>
    /// Saves all relevant analytics
    /// </summary>
    private void AllAnalytics_Save()
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
    private IEnumerator AllAnalytics_SendAndReset()
    {
        // Each player sends their personal analytics.
        LogError("[1/6]SendPlayerAnalytics() @: " + Time.time);
        SendPlayerAnalytics();

        // If the user is the master client and the number of players haven't changed, send and reset the level anyltics.
        if (IsMasterClient)
        {
            LogError("[2/6]UserCheckInWaitAndProcess @: " + Time.time);
            UserCheckInTimeout += Time.time;
            yield return StartCoroutine(WaitAndProcess_UserCheckIn());

            LogError("[3/6] DisplayUserCheckData() @: " + Time.time);
            DisplayUserCheckInData();

            LogError("[4/6]SendSessionAnalytics() @: " + Time.time);
            SendSessionAnalytics();

            LogError("[5/6]SendAssetAnalytics() @: " + Time.time);
            SendAssetAnalytics();

            LogError("[6/6] AllAnalytics_Reset() @: " + Time.time);
            AllAnalytics_Reset();
        }
    }

    /// <summary>
    /// Coroutine that runs chceck-in verification every 1/10th second until complete.
    /// </summary>
    private IEnumerator WaitAndProcess_UserCheckIn()
    {
        // How many while loops that are processed until all players check in
        int attempts = 0;

        while (!HaveAllUsersCheckedIn || Time.time > UserCheckInTimeout)
        {
            // WHAT HAPPENS IF THE PLAYER COUNT CHANGED -- IT WILL BE AN INCOMPLETE SET OF DATA THAT COULD SKEW OTHER DATA.
            if (PlayerCountChanged)
            {
                LogError("[2.1/6]Player count has changed!");

                break;
            }

            VerifyUserCheckIn();

            attempts++;

            yield return new WaitForSeconds(.1f);
        }

        LogError("[2.3/6]Number of Check-in Verification Attempts: " + attempts);
    }

    /// <summary>
    /// Verify each client has checked in with the Master Client, and finalize group analytics.
    /// </summary>
    private void VerifyUserCheckIn()
    {
        int numberOfCheckIns = 0;

        // Go through PlayerCheckIns and check if all values are true
        foreach (KeyValuePair<int, bool> pair in PlayerCheckIns)
        {
            if (pair.Value == true)
                numberOfCheckIns++;
        }

        // All players have checked in
        if (numberOfCheckIns == PlayerCount)
        {
            HaveAllUsersCheckedIn = true;
            FinalizeSaveSessionAnalytics();
            LogError("[2.2/6]All User Checked in!");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void FinalizeSaveSessionAnalytics()
    {
        AvgPlayerMoney = AllPlayersMoney / PlayerCount;
    }

    /// <summary>
    /// Resets all variables (used for a soft restart)
    /// </summary>
    private void AllAnalytics_Reset()
    {
        PlayerAnalytics_Reset();
        SessionAnalytics_Reset();
        AssetAnalytics_Reset();
    }

    /// <summary>
    /// Resets player analytics
    /// </summary>
    private void PlayerAnalytics_Reset()
    {
        PhotonViewID = 0;
        IsMasterClient = false;
        PlayerCheckIns = new Dictionary<int, bool>();
        IndividualPlayerMoney = 0;
        AllPlayersMoney = 0;
    }

    /// <summary>
    /// Resets level analytics
    /// </summary>
    private void SessionAnalytics_Reset()
    {
        TotalBallisticTaken_Tower = 0;
        TotalThraceiumTaken_Tower = 0;
        TotalBallisticTaken_Enemy = 0;
        TotalThraceiumTaken_Enemy = 0;
        GameLength = 0;
        PlayerCount = 0;
        PlayerCountChanged = false;
        //LastLevelReached = 1;
    }

    /// <summary>
    /// Reset Assets' Analytics
    /// </summary>
    private void AssetAnalytics_Reset()
    {
        Assets = new Analytics_AssetSuperclass(AssetSupertypeNames);
    }

    /// <summary>
    /// [PLAYER] Send player's analytics
    /// </summary>
    private void PlayerAnalytics_Send()
    {
        Analytics.CustomEvent(AccountID + "_PlayerStats", new Dictionary<string, object>
        {
            {"PlayerName", PlayerName},
            {"PlayerScore", PlayerScore},
            {"PlayerIncome", IndividualPlayerMoney}
        });
    }

    /// <summary>
    /// [LEVEL] Send level analytics
    /// </summary>
    private void AllLevelAnalytics_Send()
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
        //AllTowerAnalytics_Send();
        //AllEnemyAnalytics_Send();
        //AllDamageAnalytics_Send();
    }

    /// <summary>
    /// [HEATMAP] Location of asset (enemy and tower) activty on the map
    /// </summary>
    private void HeatMapAnalytics_Send()
    {
        foreach (Analytics_AssetSupertype supertype in Assets.AssetSupertypes)
        {
            foreach (Analytics_AssetSubtype subtype in supertype.AssetSubtypes)
            {
                foreach (Analytics_Asset asset in subtype.Assets)
                    HeatMapAnalyticsSingleAsset_Send(asset);
            }
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
    private void AllInitialAnalytics_Send()
    {
        //Analytics.SetUserBirthYear(1986);
        //Analytics.SetUserGender(0);
        //Analytics.SetUserId(AccountID.ToString());
        Analytics.SetUserId(PlayerName);
    }

    // [TOWERS]
    /// <summary>
    /// Send Tower Analytics to Unity's servers.
    /// </summary>
    private void AllTowerAnalytics_Send()
    {
        Analytics.CustomEvent("TowerData_Event", new Dictionary<string, object>
        {
            {"TotalBuilt_Tower", TotalBuilt_Tower},
            {"LifeSpan_Tower", TotalAvgLifeSpanDead_Tower},
        });
    }

    /// <summary>
    /// Send subtype Tower Analytics to Unity's servers.
    /// </summary>
    private void TowerSubtypeAnalytics_Send()
    {
        Analytics.CustomEvent("TowerData_Event", new Dictionary<string, object>
        {
            {"TotalBuilt_Tower", TotalBuilt_Tower},
            {"LifeSpan_Tower", TotalAvgLifeSpanDead_Tower},
        });
    }

    // [ENEMIES]
    /// <summary>
    /// Send Enemy Analytics to Unity's servers.
    /// </summary>
    private void AllEnemyAnalytics_Send()
    {
        Analytics.CustomEvent("EnemyData_Event", new Dictionary<string, object>
        {
            {"TotalSpawn_Enemy", TotalSpawn_Enemy},
            {"LifeSpan_Enemy", TotalAvgLifeSpanDead_Enemy},
        });
    }

    /// <summary>
    /// Send subtype Enemy Analytics to Unity's servers.
    /// </summary>
    private void EnemySubtypeAnalytics_Send()
    {
        Analytics.CustomEvent("EnemyData_Event", new Dictionary<string, object>
        {
            {"TotalSpawn_Enemy", TotalSpawn_Enemy},
            {"LifeSpan_Enemy", TotalAvgLifeSpanDead_Enemy},
        });
    }

    /// <summary>
    /// [DAMAGE] Level's total ballistic and thraceium damage dealt by player.
    /// </summary>
    private void AllDamageAnalytics_Send()
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

    // [MISCELLANEOUS]
    /// <summary>
    /// Send Miscellaneous Analytics custom event to Unity's servers.
    /// </summary>
    private void AllMiscellaneousAnalytics_Send()
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