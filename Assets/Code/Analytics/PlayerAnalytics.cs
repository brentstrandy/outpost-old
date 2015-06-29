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

    //LEVEL
    //public List<int> LevelWins;//Win percentage for each level for each # of players (return on disconnect or level loss)
    // TODO -- waiting on answer from Brent
    public int LastLevelReached = 1;//Avg last level reached (return on disconnect or level loss) [Send current player count with ]
    public int CurrentLevelNumber;

    //DAMAGE
    public float BallisticDamage;//Accumulated in a single level
    public float ThraceiumDamage;//Accumulated in a seingle level
    // Redundant if we send data at the end of each level
    //public List<float> totalBallisticDamage;//Avg total ballistic damage
    //public List<float> totalThraceiumDamage;//Avg total thracium damage

    //TOWERS
    // TODO: change to an array
    public int NumberBuilt_Tower;//# of towers built for each type
    public float DPS_Tower;//Avg DPS for each tower
    public float LifeSpan_Tower;//Avg lifespan for each tower
    public float DistanceFromCenter_Tower;//Avg distance from center for each tower type
    public float DistanceFromTower_Tower;//Avg distance from other tower

    //ENEMIES
    // TODO: change to an array
    public int NumberOfSpawns_Enemy;//# of spawns for each enemy type
    public float TotalDamage_Enemy;//Total damage for each enemy type
    public float DPS_Enemy;//Avg DPS for each enemy type
    public float LifeSpan_Enemy;//Avg lifespan for each enemy type

    //MONEY
    public int PlayerIncome;//Avg total income per player

    //MISCELLANEOUS
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
        CurrentLevelNumber = GetCurrentLevelNumber();
        //TowerAnalytics towarAnalytics = new TowerAnalytics();
        //EnemyAnalytics enemyAnalytics = new EnemyAnalytics();

        LevelResult();
        DamageResult();

        #region TODO: everything in this region
        //TowerResult();
        //EnemyResult();
        //MoneyResult();
        //MiscellaneousResult();
        #endregion
    }

    // Resets variables that accumulate in a single level
    public void ResetLevelStats()
    {
        BallisticDamage = 0;
        ThraceiumDamage = 0;
    }

    // Resets all variables (used for a soft restart)
    public void ResetEverything()
    {
        ResetLevelStats();

        LastLevelReached = 1;
    }

    // [LEVEL] Player's last level reached and the amount of players participating.
    private void LevelResult()
    {
        Analytics.CustomEvent("LastLevelReached_Event", new Dictionary<string, object>
        {
            {"LastLevelReached", LastLevelReached},
            {"CurrentPlayerCount", SessionManager.Instance.GetRoomPlayerCount()}
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
            {"LevelNumber", CurrentLevelNumber}
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
            {" DPS_Enemy",  DPS_Enemy},
            {"LifeSpan_Enemy", LifeSpan_Enemy},
        });
    }

    // [MONEY] 
    private void MoneyResult()
    {
        Analytics.CustomEvent("MoneyData_Event", new Dictionary<string, object>
        {
            {"PlayerIncome", PlayerIncome}
        });
    }

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
    public void SendInitialStats()
    {
        //Analytics.SetUserBirthYear(1986);
        //Analytics.SetUserGender(0);
        //Analytics.SetUserId("ID Number");
    }

    // Determines the player's current level number based on the level's Display Name (must contain a digit).
    private int GetCurrentLevelNumber()
    {
        int tempLevelNumber;
        string tempLevelName = GameManager.Instance.CurrentLevelData.DisplayName;

        tempLevelName = tempLevelName.Substring(tempLevelName.IndexOf(" ") + 1); ;
        bool validNumber = Int32.TryParse(tempLevelName, out tempLevelNumber);

        if (validNumber)
            return tempLevelNumber;
        else
        {
            LogError("Invalid Level Number");
            return 0;
        }
    }

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