using UnityEngine;
using System.Collections;

/// <summary>
/// A single asset that gets collected for analytics inside a List<Analytics_TrackedAssets>
/// Owner: John Fitzgerald
/// </summary>
public class Analytics_Asset
{
    public int ViewID;
    public string AssetType;             // Enemy or Tower

    private float DPS;                   // Thraceium and Ballistic combined?
    private float DamageTaken;           // Thraceium and Ballistic combined?
    private float DistanceFromCenter;    // Tower at the time of creation and enemy at the time of death.
    private float DistanceFromSameType;  // Enemy to Enemy or only sub type to sub type (eg. TRT to TRT)?
    private float StartOfLife;
    private float LifeSpan;
    public Vector3 AssetOrigin;
    private Vector3 LocationOfDeath;     // Use to coordinate distance from other assets.
    private Vector3 MiningFacility
    {
        get
        {
            return GameObject.FindGameObjectWithTag("Mining Facility").transform.position;
        }
    }

    // constructor for enemies
    public Analytics_Asset(int viewID, string assetType)
    {
        ViewID = viewID;
        AssetType = assetType;

        DPS = 0;
        DamageTaken = 0;
        DistanceFromCenter = 0;
        DistanceFromSameType = 0;
        StartOfLife = Time.time;
        LifeSpan = 0;
    }

    // constructor for towers
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
    public void CalculateDistanceFromCenter()
    {
        if (AssetType == "Enemy")
            DistanceFromCenter = Vector3.Distance(MiningFacility, LocationOfDeath);
        else
            DistanceFromCenter = Vector3.Distance(MiningFacility, AssetOrigin);
    }

    /// <summary>
    /// Distance from other assets of the same type (sub-type?)
    /// </summary>
    public void CalculateDistanceFromSameType()
    {

    }
}
