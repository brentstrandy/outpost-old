using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class PlayerAnalytics : MonoBehaviour
{
    private static PlayerAnalytics instance;
    public bool ShowDebugLogs = true;

    //LEVEL
    // make dynamic for players joining/leaving
    public List<bool[]> levelWins;//??Win percentage for each level for each # of players
    public int lastLevelReached;//Avg last level reached

    //DAMAGE
    public float totalBallisticDamage;//Avg total ballistic damage
    public float totalThraceiumDamage;//Avg total thracium damage

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
    #endregion

    // Use this for initialization
    public void Start() 
    {
        // initialize based on number of levels
        levelWins = new List<bool[]>(GameDataManager.Instance.LevelDataManager.DataList.Count);
        lastLevelReached = 1;

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