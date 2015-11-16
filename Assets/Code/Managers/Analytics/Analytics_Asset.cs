using UnityEngine;
using System.Collections;

/// <summary>
/// Holds a single Asset as part of a Subtype
/// Owner: John Fitzgerald
/// </summary>
public class Analytics_Asset
{
    public bool ShowDebugLogs = true;
    public bool IsMiningFacility { get; private set; }      // Is the asset the "Mining Facility"
    public bool IsDead { get; private set; }                // Is the asset dead

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

        TimeOfSpawnSinceLoad = Time.time;
        TimeOfDeathSinceLoad = 0;
        LifeSpan = 0;

        LocationOfSpawn = locationOfSpawn;

        IsMiningFacility = viewID == 10 ? true : false;     // The Mining Facility viewID is always 10
        IsDead = false;
    }

    /// <summary>
    /// Adds amount of damage the asset deals during Fire
    /// </summary>
    public void AddDamageDealt(float ballistic, float thraceium)
    {   
        DamageDealt_Ballistic += ballistic;
        DamageDealt_Thraceium += thraceium;
    }

    /// <summary>
    /// Adds amount of damage the asset takes while being Fired on
    /// </summary>
    public void AddDamageTaken(float ballistic, float thraceium)
    {
        DamageTaken_Ballistic += ballistic;
        DamageTaken_Thraceium += thraceium;
    }

    /// <summary>
    /// Information stored after the asset is declared dead
    /// </summary>
    public void DeathOfAsset(Vector3 locationOfDeath)
    {
        if (GameManager.Instance.GameRunning)
        {
            TimeOfDeathSinceLoad = Time.time;
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
        Vector3 MFLocation = AnalyticsManager.Instance.MiningFacilityLocation;

        if (AssetSupertype == "Enemy" && IsDead)
            DistanceFromCenter = Vector3.Distance(MFLocation, LocationOfDeath);
        else if (AssetSupertype == "Tower")
            DistanceFromCenter = Vector3.Distance(MFLocation, LocationOfSpawn);
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
