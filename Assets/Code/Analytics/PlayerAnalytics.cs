using UnityEngine;
using UnityEngine.Analytics;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A collection of analytics for the player and game
/// Owner: John Fitzgerald
/// </summary>
public class PlayerAnalytics : MonoBehaviour
{
    private static PlayerAnalytics instance;
    public bool ShowDebugLogs = true;

    /// <summary>
    ///         GENERAL RULES:
    /// 1) Only send CustomEvents at the end of a match.
    /// 
    ///         TASK AT HAND:
    /// 1) For sending global ("ALL") data: only send data from client if they are the master client.
    /// 2) Prioritize variables based on Justin's Asana ordering ([H]igh or [L]ow).
    /// 3) Create the averages on the user side to avoid sending so many events to the server.
    /// 4) Figure out if I need to send the Player's name for every CustomEvent, or just once upfront:
    ///    this will be used to categorize all of the user's data.
    /// 
    ///         NEW VARIABLES:
    /// 1) &[]Percentage of time the enemies are on () (ALL)
    /// 2) &[]Find the amount of damage for each enemy to tower (ALL)
    /// 3) &[]Use user's name to categorize their data (SINGLE)
    /// 4) Win percentage for each level for each # of players (ALL)
    /// </summary>

    //[PLAYER]
    public string PlayerName;//Name derived from (Player.Instance.Name)
    public float PlayerIncome;//Avg total income per player
    public bool IsMaster = false;//Indicate if the player is the master client (they will send the global data)

    //[LEVEL]
    public int LastLevelReached = 1;//Avg last level reached.
    public int PlayerCount;//Playercount at the beginning of a level.
    public bool PlayerCountChanged;//indicates if the player count has changed since the beginning of a level.
    //public int CurrentLevelNumber;//Copy of LevelID?
    public float GameLength;//Length of time the user plays the level (from game start to loss/win)
    public int LevelID;//ID number of level

    //[DAMAGE]
    public float BallisticDamage;//Accumulated in a single level
    public float ThraceiumDamage;//Accumulated in a seingle level
    // Redundant if we send data at the end of each level
    //public List<float> totalBallisticDamage;//Avg total ballistic damage
    //public List<float> totalThraceiumDamage;//Avg total thracium damage

    //[TOWERS]
    // TODO: change to an array
    public int NumberBuilt_Tower;//# of towers built for each type
    public float DPS_Tower;//Avg DPS for each tower
    public float LifeSpan_Tower;//Avg lifespan for each tower
    public float DistanceFromCenter_Tower;//Avg distance from center for each tower type
    public float DistanceFromTower_Tower;//Avg distance from other tower
    //TODO
    //&[]Percentage of time the towers are on () (SINGLE)
    //TODO
    //ADD A HEAT MAP (or collect data to create one -- "Unity Heat Map Tool")

    //[ENEMIES]
    // TODO: change to an array
    public int NumberOfSpawns_Enemy;//# of spawns for each enemy type
    public float TotalDamage_Enemy;//Total damage for each enemy type
    public float DPS_Enemy;//Avg DPS for each enemy type
    public float LifeSpan_Enemy;//Avg lifespan for each enemy type

    //[MISCELLANEOUS]
    public int StolenThraceium;//Avg thraceium stolen per player
    public int NumberOfBarriers;//Avg # of barriers placed per player

    #region INSTANCE (SINGLETON)
    /// <summary>
    /// Singleton - There can only be one
    /// </summary>
    /// <value>The instance.</value>
    public static PlayerAnalytics Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<PlayerAnalytics>();
            }

            return instance;
        }
    }

    void Awake()
    {
        instance = this;
    }
    #endregion

    #region TODO (FITZGERALD)
    // Used for TowerResult()
    private struct TowerAnalytics
    {
        //public TowerAnalytics()
        //{

        //}
    }

    // Used for EnemyResult()
    private struct EnemyAnalytics
    {
        //public EnemyAnalytics()
        //{

        //}
    }
    #endregion

    // Called at the beginning of each level
    public void InitializePlayerStats()
    {
        PlayerName =  PlayerManager.Instance.Name;

        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
            IsMaster = true;
    }

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
    public void SendLevelStats()
    {
        LevelID = GameManager.Instance.CurrentLevelData.LevelID;//GetCurrentLevelNumber();
        
        // TODO -- indicate that the player count changed at some point during the game
        if (PlayerCountChanged)
        {
            //changes in GameManager.Instace.OnPlayerLeft()
        }
        else
            PlayerCount = SessionManager.Instance.GetRoomPlayerCount();

        // TODO -- figure out which variables to store (dependent on if they're ALL or SINGLE)
        //TowerAnalytics towarAnalytics = new TowerAnalytics();
        //EnemyAnalytics enemyAnalytics = new EnemyAnalytics();

        // TODO: should this get called when the player loses? Seems like a waste to send at the end of every game.
        LevelResult();
        DamageResult();

        #region TODO: everything in this region
        //TowerResult();
        //EnemyResult();
        //MoneyResult();//changed to PlayerStats()
        //MiscellaneousResult();
        #endregion
    }

    // 
    public void SendPlayerStats()
    {
        PlayerResult();
    }

    // Resets all variables (used for a soft restart)
    public void ResetEverything()
    {
        ResetLevelStats();
        ResetPlayerStats();

        LastLevelReached = 1;
    }

    // Resets variables that accumulate in a single level
    public void ResetLevelStats()
    {
        BallisticDamage = 0;
        ThraceiumDamage = 0;
        GameLength = 0;
        PlayerCount = 0;
        PlayerCountChanged = false;
    }
    
    // Resets variables that the only pertain to the Player
    public void ResetPlayerStats()
    {
        // TODO -- account for the possibility the player changes his/her name?
        PlayerIncome = 0;
        IsMaster = false;
    }

    // [PLAYER] Player's stats
    private void PlayerResult()
    {
        PlayerIncome = PlayerManager.Instance.Money; // Amount of money at the end of the match

        Analytics.CustomEvent("PlayerStats", new Dictionary<string, object>
        {
            {"PlayerIncome", PlayerIncome},
            {"PlayerName", PlayerManager.Instance.Name}
        });
    }

    // [LEVEL] Player's last level reached and the amount of players participating.
    private void LevelResult()
    {
        Analytics.CustomEvent("LastLevelReached_Event", new Dictionary<string, object>
        {
            {"LastLevelReached", LastLevelReached},
            {"CurrentPlayerCount", PlayerCount},
            {"GameLength", GameLength},
            {"LevelName", GameManager.Instance.CurrentLevelData.SceneName},
            {"LevelID", LevelID},
            {"PlayerName", PlayerManager.Instance.Name}
        });
    }

    // [DAMAGE] Level's total ballistic and thraceium damage dealt by player.
    private void DamageResult()
    {
        Analytics.CustomEvent("TotalLevelDamage_Event", new Dictionary<string, object>
        {
            {"BallisticDamage", BallisticDamage},
            {"ThraceiumDamage", ThraceiumDamage},
            {"LevelName", GameManager.Instance.CurrentLevelData.SceneName},
            {"LevelID", LevelID}
        });
    }

    #region TODO (FITZGERALD)
    // [TOWERS] 
    private void TowerResult()
    {
        Analytics.CustomEvent("TowerData_Event", new Dictionary<string, object>
        {
            {"NumberBuilt_Tower", NumberBuilt_Tower},
            {"DPS_Tower", DPS_Tower},
            {"LifeSpan_Tower", LifeSpan_Tower},
            {"DistanceFromCenter_Tower", DistanceFromCenter_Tower},
            {"DistanceFromTower_Tower", DistanceFromTower_Tower}
        });
    }

    // [ENEMIES] 
    private void EnemyResult()
    {
        Analytics.CustomEvent("EnemyData_Event", new Dictionary<string, object>
        {
            {"NumberOfSpawns_Enemy", NumberOfSpawns_Enemy},
            {"TotalDamage_Enemy", TotalDamage_Enemy},
            {"DPS_Enemy",  DPS_Enemy},
            {"LifeSpan_Enemy", LifeSpan_Enemy},
        });
    }

    //// [MONEY] 
    //private void MoneyResult()
    //{
    //    Analytics.CustomEvent("MoneyData_Event", new Dictionary<string, object>
    //    {
    //        {"PlayerIncome", PlayerIncome}
    //    });
    //}

    // TODO: Fix naming for these variables ("Miscellaneous" will not work)
    // [MISCELLANEOUS] 
    private void MiscellaneousResult()
    {
        Analytics.CustomEvent("Miscellaneous_Event", new Dictionary<string, object>
        {
            {"StolenThraceium", StolenThraceium},
            {"NumberOfBarriers", NumberOfBarriers}        
        });
    }
    #endregion

    // Optional stats for game.
    private void SendInitialStats()
    {
        //Analytics.SetUserBirthYear(1986);
        //Analytics.SetUserGender(0);
        //Analytics.SetUserId("ID Number");
    }

    //// Determines the player's current level number based on the level's Display Name (must contain a digit).
    //private int GetCurrentLevelNumber()
    //{
    //    int tempLevelNumber;
    //    string tempLevelName = GameManager.Instance.CurrentLevelData.DisplayName;

    //    tempLevelName = tempLevelName.Substring(tempLevelName.IndexOf(" ") + 1); ;
    //    bool validNumber = Int32.TryParse(tempLevelName, out tempLevelNumber);

    //    if (validNumber)
    //        return tempLevelNumber;
    //    else
    //    {
    //        LogError("Invalid Level Number");
    //        return 0;
    //    }
    //}

    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[PlayerAnalytics] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[PlayerAnalytics] " + message);
    }
    #endregion
}