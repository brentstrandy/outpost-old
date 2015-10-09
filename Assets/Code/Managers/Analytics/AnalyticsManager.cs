using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

/// <summary>
/// A collection of analytics for the player and game
/// Owner: John Fitzgerald
/// </summary>
public class AnalyticsManager : MonoBehaviour
{
    private static AnalyticsManager instance;
    public bool ShowDebugLogs = true;

    // Send all the level stats to the Unity Analaytics server.
    // *************************************************
    //http://docs.unity3d.com/Manual/UnityAnalyticsCustomAttributes51.html
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
    /// 1) For sending global "(MASTER)" data: only send data from master client.
    /// 2) Compute most averages on the user side to avoid sending so many events to the server.
    /// 3) Figure out if I need to send the Player's name for every CustomEvent, or just once upfront:
    ///    this will be used to categorize all of the user's data.
    /// </summary>

    //[PLAYER]
    public string PlayerName;//Name derived from (Player.Instance.Name) (ALL)
    public float PlayerMoney;//Total income per player (ALL & MASTER)
    public bool IsMaster = false;//Indicate if the player is the master client (they will send the global data) (MASTER)

    //[AVAILABLE ENEMIES AND TOWERS]
    public List<Analytics_TrackedAssets> AvailableTowers;
    public List<Analytics_TrackedAssets> AvailableEnemies;
    
    //[LEVEL]
    // public float WinPercent_Level; // Win percentage for each level for each # of players (ALL)
    public int LastLevelReached = 0;//Avg last level reached. (ALL)
    public int PlayerCount;//Playercount at the beginning of a level. (ALL & MASTER)
    public bool PlayerCountChanged;//Indicates if the player count has changed since the beginning of a level (ALL & MASTER)
    public float GameLength;//Length of time the user plays the level (from game start to loss/win) (MASTER)
    public int LevelID;//ID number of level (MASTER)

    //[DAMAGE BY LEVEL]
    public float BallisticDamage_Level;//Accumulated in a single level (ALL & MASTER)
    public float ThraceiumDamage_Level;//Accumulated in a seingle level (ALL & MASTER)

    ////[TOWERS]
    //private Dictionary<string, int> NumberBuilt_Tower;
    ////public int[] NumberBuilt_Tower;//# of towers built for each type (ALL & MASTER)
    //private Dictionary<string, float> DPS_Tower;
    ////public float[] DPS_Tower;//Avg DPS for each tower (ALL & MASTER)
    //private Dictionary<string, float> DistanceFromCenter_Tower;
    ////public float[] DistanceFromCenter_Tower;//Avg distance from center for each tower type (ALL & MASTER)
    //private Dictionary<string, float> DistanceFromTower_Tower;
    ////public float[][] DistanceFromTower_Tower;//Avg distance from other tower (ALL & MASTER)
    //private Dictionary<string, float> LifeSpan_Tower;
    ////public float[] LifeSpan_Tower;//Avg lifespan for each tower (ALL & MASTER)

    ////[ENEMIES]
    //public int[] NumberOfSpawns_Enemy;//# of spawns for each enemy type (This is controlled by us -- do we need to track it?) (ALL & MASTER)
    //public float[] TotalDamage_Enemy;//Total damage from each tower type (ALL & MASTER)
    //public float[] DPS_Enemy;//Avg DPS for each enemy type (ALL & MASTER)
    //public float[] LifeSpan_Enemy;//Avg lifespan for each enemy type that is destroyed (ALL & MASTER)
    //public float[] DamageToTower_Enemy;// Find the amount of damage for each enemy to tower (ALL & MASTER)
    ////public float[] ActiveTime_Enemy;//Percentage of time that each of the enemies are active (ALL)

    //[MISCELLANEOUS]
    public int StolenThraceium;//Avg thraceium stolen per player ()
    public int NumberOfBarriers;//Avg # of barriers placed per player ()

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
    /// Called at the beginning of each level within GameManager.StartGame()
    /// </summary>
    public void InitializePlayerAnalytics()
    {
        PlayerName = PlayerManager.Instance.Username;
        PlayerCount = SessionManager.Instance.GetRoomPlayerCount();

        // Create list of available towers
        foreach (TowerData tower in GameDataManager.Instance.TowerDataManager.DataList)
            AvailableTowers.Add(new Analytics_TrackedAssets(tower.DisplayName));
        // Create list of available enemies
        foreach (EnemyData enemy in GameDataManager.Instance.EnemyDataManager.DataList)
            AvailableEnemies.Add(new Analytics_TrackedAssets(enemy.DisplayName));

        // Determine Master Client
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
            IsMaster = true;
    }

    /// <summary>
    /// Save Player analytics
    /// </summary>
    public void SavePlayerAnalytics()
    {
        PlayerMoney = PlayerManager.Instance.Money;
    }

    /// <summary>
    /// Save Session/Level analytics
    /// </summary>
    public void SaveSessionAnalytics()
    {
        GameLength = Time.time - GameManager.Instance.LevelStartTime;
        LevelID = GameManager.Instance.CurrentLevelData.LevelID;
        LastLevelReached = GameManager.Instance.CurrentLevelData.LevelID;

        //if (PlayerCountChanged)
        //{
        //    //changes in GameManager.Instace.OnPlayerLeft()
        //}
        //else
        //    PlayerCount = SessionManager.Instance.GetRoomPlayerCount();

        // TODO -- (FITZGERALD) -- Analytics -- discard Analytics because player count changed, or create subgroup of "player dropped" data (refer to Justin).
        //if (AnalyticsManager.Instance.PlayerCountChanged)

        // TODO -- (FITZGERALD) -- average all of the players' money
        //AnalyticsManager.Instance.PLayerIncome_Average =;

        // TODO -- (FITZGERALD) -- figure out which variables to store (dependent on if they're ALL or SINGLE)
        //TowerAnalytics towarAnalytics = new TowerAnalytics();
        //EnemyAnalytics enemyAnalytics = new EnemyAnalytics();
    }

    /// <summary>
    /// Save Asset analytics
    /// </summary>
    public void SaveAssetAnalytics()
    {

    }

    /// <summary>
    /// Public call to send session/level analytics.
    /// </summary>
    public void SendSessionAnalytics()
    {
        LevelAnalytics_Send();
        DamageAnalytics_Send();
        //TowerAnalytics_Send();
        DamageAnalytics_Send();
        MiscellaneousAnalytics_Send();
        InitialAnalytics_Send();
    }

    /// <summary>
    /// Public call to send Player analytics.
    /// </summary>
    public void SendPlayerAnalytics()
    {
        PlayerAnalytics_Send();
    }

    /// <summary>
    /// Public call to send Asset analytics.
    /// </summary>
    public void SendPlayerAnalytics()
    {
        AssetAnalytics_Send();
    }

    #endregion PUBLIC FUNCTIONS

    #region RESET ANALYTICS
    /// <summary>
    /// Resets level analytics
    /// </summary>
    public void ResetLevelAnalytics()
    {
        BallisticDamage_Level = 0;
        ThraceiumDamage_Level = 0;
        GameLength = 0;
        PlayerCount = 0;
        PlayerCountChanged = false;
        LastLevelReached = 1;
    }

    /// <summary>
    /// Resets player analytics
    /// </summary>
    public void ResetPlayerAnalytics()
    {
        PlayerMoney = 0;
        IsMaster = false;
    }

    /// <summary>
    /// Reset asset analytics
    /// </summary>
    public void ResetAssetAnalytics()
    {
        AvailableTowers = new List<Analytics_TrackedAssets>();
        AvailableEnemies = new List<Analytics_TrackedAssets>();
    }

    /// <summary>
    /// Resets all variables (used for a soft restart)
    /// </summary>
    public void ResetAllAnalytics()
    {
        ResetLevelAnalytics();
        ResetPlayerAnalytics();
        ResetAssetAnalytics();
    }
    #endregion RESET ANALYTICS

    #region SEND ANALYTICS
    /// <summary>
    /// [PLAYER] Send player's analytics
    /// </summary>
    private void PlayerAnalytics_Send()
    {
        Analytics.CustomEvent("PlayerStats", new Dictionary<string, object>
        {
            {"PlayerName", PlayerManager.Instance.Username},
            {"PlayerIncome", PlayerMoney}
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
            {"LevelID", LevelID},
            {"PlayerName", PlayerManager.Instance.Username}
        });
    }

    /// <summary>
    /// [ASSETS] Send assets analytics
    /// </summary>
    private void AssetAnalytics_Send()
    {

    }

    // Combine with LevelAnalytics?
    /// <summary>
    /// [DAMAGE] Level's total ballistic and thraceium damage dealt by player.
    /// </summary>
    private void DamageAnalytics_Send()
    {
        Analytics.CustomEvent("TotalLevelDamage_Event", new Dictionary<string, object>
        {
            {"BallisticDamage", BallisticDamage_Level},
            {"ThraceiumDamage", ThraceiumDamage_Level},
            {"LevelName", GameManager.Instance.CurrentLevelData.SceneName},
            {"LevelID", LevelID}
        });
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

    #region TODO -- (FITZGERALD)

    //// [TOWERS]
    //private void TowerAnalytics_Send()
    //{
    //    Analytics.CustomEvent("TowerData_Event", new Dictionary<string, object>
    //    {
    //        {"NumberBuilt_Tower", NumberBuilt_Tower},
    //        {"DPS_Tower", DPS_Tower},
    //        {"LifeSpan_Tower", LifeSpan_Tower},
    //        {"DistanceFromCenter_Tower", DistanceFromCenter_Tower},
    //        {"DistanceFromTower_Tower", DistanceFromTower_Tower}
    //    });
    //}

    //// [ENEMIES]
    //private void EnemyAnalytics_Send()
    //{
    //    Analytics.CustomEvent("EnemyData_Event", new Dictionary<string, object>
    //    {
    //        {"NumberOfSpawns_Enemy", NumberOfSpawns_Enemy},
    //        {"TotalDamage_Enemy", TotalDamage_Enemy},
    //        {"DPS_Enemy",  DPS_Enemy},
    //        {"LifeSpan_Enemy", LifeSpan_Enemy},
    //    });
    //}

    // TODO -- (FITZGERALD) -- Fix naming for these variables ("Miscellaneous" will not work)
    // [MISCELLANEOUS]
    private void MiscellaneousAnalytics_Send()
    {
        Analytics.CustomEvent("Miscellaneous_Event", new Dictionary<string, object>
        {
            {"StolenThraceium", StolenThraceium},
            {"NumberOfBarriers", NumberOfBarriers}
        });
    }

    #endregion TODO -- (FITZGERALD)
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