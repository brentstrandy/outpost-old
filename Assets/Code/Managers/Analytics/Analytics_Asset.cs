using UnityEngine;
using System.Collections;

/// <summary>
/// Holds a single Asset as part of a Subtype
/// Owner: John Fitzgerald
/// </summary>
public class Analytics_Asset
{
    public bool ShowDebugLogs = true;

    public int ViewID { get; private set; }                 // The asset's unique View ID
    public string AssetSupertype { get; private set; }      // e.g. "Enemy" or "Tower"
    public string AssetSubtype { get; private set; }        // e.g. "Light Speeder" or "EMP"

    public float DamageDealt_Ballistic { get; private set; } // Raw ddb
    public float DamageDealt_Thraceium { get; private set; } // Raw ddt
    public float DamageTaken_Ballistic { get; private set; } // Raw dtb
    public float DamageTaken_Thraceium { get; private set; } // Raw dtt

    public float DistanceFromCenter { get; private set; }    // Tower: at the time of creation -- Enemy: at the time of death
    public float TimeOfSpawnSinceLoad { get; private set; }  // How long since the level loaded and the asset spawned
    public float TimeOfDeathSinceLoad { get; private set; }  // How long since the level loaded and the asset died
    public float LifeSpan { get; private set; }              // When the asset is declared dead (e.g. health < 0)

    public Vector3 LocationOfSpawn { get; private set; }    // Where the asset spawned onto the map
    public Vector3 LocationOfDeath { get; private set; }    // Use to coordinate distance from other assets
    private Vector3 MiningFacilityLocation                  // Returns the GameManager's location of the Mining Facility
    {
        get
        {
            return GameManager.Instance.ObjMiningFacility.transform.position;
        }
    }
    public bool IsMiningFacility { get; private set; }      // Is the asset the "Mining Facility"
    public bool IsDead { get; private set; }                // Is the asset dead

    public Analytics_Asset(int viewID, string assetSupertype, string assetSubtype, Vector3 locationOfSpawn)
    {
        ViewID = viewID;
        AssetSupertype = assetSupertype;
        AssetSubtype = assetSubtype;

        DamageDealt_Ballistic = 0;
        DamageDealt_Thraceium = 0;
        DamageTaken_Ballistic = 0;
        DamageTaken_Thraceium = 0;
        
        DistanceFromCenter = 0;

        TimeOfSpawnSinceLoad = Time.time - GameManager.Instance.LevelStartTime;
        TimeOfDeathSinceLoad = 0;
        LifeSpan = 0;

        LocationOfSpawn = locationOfSpawn;

        IsMiningFacility = viewID == 10 ? true : false;     // The Mining Facility viewID is always 10
        IsDead = false;
    }

    /// <summary>
    /// Amount of damage asset deals
    /// </summary>
    public void AddDamageDealt(float ballistic, float thraceium)
    {   
        DamageDealt_Ballistic += ballistic;
        DamageDealt_Thraceium += thraceium;
    }

    /// <summary>
    /// Amount of damage asset takes
    /// </summary>
    public void AddDamageTaken(float ballistic, float thraceium)
    {
        DamageTaken_Ballistic += ballistic;
        DamageTaken_Thraceium += thraceium;
    }

    /// <summary>
    /// Death location and lifespan of asset
    /// </summary>
    public void DeathOfAsset(Vector3 locationOfDeath)
    {
        if (GameManager.Instance.GameRunning)
        {
            TimeOfDeathSinceLoad = Time.time - GameManager.Instance.LevelStartTime;
            LifeSpan = TimeOfDeathSinceLoad - TimeOfSpawnSinceLoad;
            LocationOfDeath = locationOfDeath;
            IsDead = true;
        }
    }

    /// <summary>
    /// Distance from center of Mining Facility after the asset dies
    /// </summary>
    public float GetDistanceFromCenter()
    {
        if (AssetSupertype == "Enemy" && IsDead)
            DistanceFromCenter = Vector3.Distance(MiningFacilityLocation, LocationOfDeath);
        else if (AssetSupertype == "Tower")
            DistanceFromCenter = Vector3.Distance(MiningFacilityLocation, LocationOfSpawn);
        else
            LogError("Incorrect Type of asset is being tracked: " + AssetSupertype);

        return DistanceFromCenter;
    }

    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[Analytics_Asset] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[Analytics_Asset] " + message);
    }
    #endregion MessageHandling
}
