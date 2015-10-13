using UnityEngine;
using System.Collections;

/// <summary>
/// A single asset that gets collected for analytics inside a List<Analytics_TrackedAssets>
/// Owner: John Fitzgerald
/// </summary>
public class Analytics_Asset
{
    public bool ShowDebugLogs = true;

    public int ViewID;
    public string AssetType;             // Enemy or Tower

    private float DPS;                   // Thraceium and Ballistic combined?
    private float DamageTaken;           // Thraceium and Ballistic combined?
    private float DistanceFromCenter;    // Tower at the time of creation and enemy at the time of death.
    private float DistanceFromSameType;  // Enemy to Enemy or only sub type to sub type (eg. TRT to TRT)?
    private float StartOfLife;
    private float LifeSpan;
    public Vector2 AssetOrigin;
    private Vector3 LocationOfDeath;     // Use to coordinate distance from other assets.
    private Vector3 MiningFacility
    {
        get
        {
            return GameObject.FindGameObjectWithTag("Mining Facility").transform.position;
        }
    }

    // constructor
    public Analytics_Asset(int viewID, string assetType, Vector3 assetOrigin)
    {
        ViewID = viewID;
        AssetType = assetType;

        DPS = 0;
        DamageTaken = 0;
        DistanceFromCenter = 0;
        DistanceFromSameType = 0;
        StartOfLife = Time.time;
        LifeSpan = 0;
        AssetOrigin = assetOrigin;
    }

    /// <summary>
    /// Amount of damage asset deals
    /// </summary>
    public void AddDPS(float dps)
    {
        DPS += dps;
    }

    /// <summary>
    /// Amount of damage asset takes
    /// </summary>
    public void AddDamageTaken(float damageTaken)
    {
        DamageTaken += damageTaken;
    }

    /// <summary>
    /// Death location and lifespan of asset
    /// </summary>
    public void DeathOfAsset(Vector3 locationOfDeath)
    {
        LifeSpan = Time.time - StartOfLife;
        LocationOfDeath = locationOfDeath;
    }

    /// <summary>
    /// Distance from center of Mining Facility after the asset dies
    /// </summary>
    public float GetDistanceFromCenter()
    {
        if (AssetType == "Enemy")
            DistanceFromCenter = Vector3.Distance(MiningFacility, LocationOfDeath);
        else if (AssetType == "Tower")
            DistanceFromCenter = Vector3.Distance(MiningFacility, AssetOrigin);
        else
            LogError("Incorrect Type of asset is being tracked: " + AssetType);

        return DistanceFromCenter;
    }

    /// <summary>
    /// Distance from other assets of the same type
    /// </summary>
    public void CalculateDistanceFromSameType()
    {

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
