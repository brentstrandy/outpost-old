using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;

/// <summary>
/// A collection of analytics for the player and game
/// Owner: John Fitzgerald
/// </summary>
public class PlayerAnalytics : MonoBehaviour
{
    private static PlayerAnalytics instance;
    public bool ShowDebugLogs = true;

    //LEVEL
    public List<int> levelWins;//Win percentage for each level for each # of players (return on disconnect or level loss)
    public int lastLevelReached = 1;//Avg last level reached (return on disconnect or level loss)

    //DAMAGE
    public float ballisticDamage;//Accumulated in a single level
    public float thraceiumDamage;//Accumulated in a seingle level
    // Redundant if we send data at the end of each level
    //public List<float> totalBallisticDamage;//Avg total ballistic damage
    //public List<float> totalThraceiumDamage;//Avg total thracium damage

    //TOWERS
    // change to an array
    public int numberBuilt_Tower;//# of towers built for each type
    public float DPS_Tower;//Avg DPS for each tower
    public float lifeSpan_Tower;//Avg lifespan for each tower
    public float distanceFromCenter_Tower;//Avg distance from center for each tower type
    public float distanceFromTower_Tower;//Avg distance from other tower

    //ENEMIES
    // change to an array
    public int numberOfSpawns_Enemy;//# of spawns for each enemy type
    public float totalDamage_Enemy;//Total damage for each enemy type
    public float DPS_Enemy;//Avg DPS for each enemy type
    public float lifeSpan_Enemy;//Avg lifespan for each enemy type

    //MONEY
    public int playerIncome;//Avg total income per player

    //MISCELLANEOUS
    public int stolenThraceium;//Avg thraceium stolen per player
    public int numberOfBarriers;//Avg # of barriers placed per player

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

    // Use this for initialization
    public void Start() 
    {
        
	}

    // Resets variables that accumulate in a single level
    public void ResetLevelStats()
    {
        ballisticDamage = 0;
        thraceiumDamage = 0;
    }

    // Send all the level stats to the Unity Analaytics server
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
        // send levels total ballistic and thraceium damage
        Analytics.CustomEvent("TotalLevelDamage", new Dictionary<string, object>
        {
            {"ballisticDamage", ballisticDamage},
            {"thraceiumDamage", thraceiumDamage},
            //{"levelName", GameManager.Instance.CurrentLevelData.SceneName}// to categorize data (if Justin deems necessary)
        });

    }

    // Optional stats for game
    public void SendInitialStats()
    {
        //Analytics.SetUserBirthYear(1986);
        //Analytics.SetUserGender(0);
        //Analytics.SetUserId("ID Number");
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