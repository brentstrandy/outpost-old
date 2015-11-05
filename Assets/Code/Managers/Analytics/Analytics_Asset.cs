using UnityEngine;
using System.Collections;

/// <summary>
/// A single asset that gets collected for analytics inside a List<Analytics_TrackedAssets>
/// Owner: John Fitzgerald
/// </summary>
public class Analytics_Asset
{
    public bool ShowDebugLogs = true;

    public int ViewID;                   // The asset's unique View ID
    public string AssetType;             // "Enemy" or "Tower" (for now)
    public float DamageDealt_Ballistic { get; private set; } // Raw ddb
    public float DamageDealt_Thraceium { get; private set; } // Raw ddt
    public float DamageTaken_Ballistic { get; private set; } // Raw dtb
    public float DamageTaken_Thraceium { get; private set; } // Raw dtt
    
    private float DistanceFromCenter;    // Tower: at the time of creation -- Enemy: at the time of death
    private float StartOfLife;           // The start of the asset's creation
    public float LifeSpan { get; private set; }             // When the asset is declared dead (e.g. health < 0)

    public bool IsDead { get; private set; }                // Is the asset dead

    public Vector2 LocationOfSpawn;          // Where the asset spawned onto the map
    public Vector2 LocationOfDeath { get; private set; }    // Use to coordinate distance from other assets
    private Vector2 MiningFacility       // Returns the GameManager's location of the Mining Facility
    {
        get
        {
            return GameManager.Instance.ObjMiningFacility.transform.position;
        }
    }
    public bool IsMiningFacility { get; private set; }

    // constructor
    public Analytics_Asset(int viewID, string assetType, Vector3 assetOrigin)
    {
        ViewID = viewID;
        AssetType = assetType;

        DamageDealt_Thraceium = 0;
        DamageDealt_Ballistic = 0;
        DamageTaken_Thraceium = 0;
        DamageTaken_Ballistic = 0;
        
        DistanceFromCenter = 0;
        StartOfLife = Time.time;
        LifeSpan = 0;

        IsDead = false;

        LocationOfSpawn = assetOrigin;

        IsMiningFacility = viewID == 10 ? true : false; // Mining facility viewID is always 10
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
            LifeSpan = Time.time - StartOfLife;
            LocationOfDeath = locationOfDeath;
            IsDead = true;
        }
    }

    /// <summary>
    /// Distance from center of Mining Facility after the asset dies
    /// </summary>
    public float GetDistanceFromCenter()
    {
        if (AssetType == "Enemy")
            DistanceFromCenter = Vector3.Distance(MiningFacility, LocationOfDeath);
        else if (AssetType == "Tower")
            DistanceFromCenter = Vector3.Distance(MiningFacility, LocationOfSpawn);
        else
            LogError("Incorrect Type of asset is being tracked: " + AssetType);

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
