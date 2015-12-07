using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

/// <summary>
/// Holds a List of the available Supertypes ("Tower", "Enemy", etc.) in a Superclass
/// Owner: John Fitzgerald
/// </summary>
public class Analytics_AssetSuperclass
{
    public bool ShowDebugLogs = true;

    public List<Analytics_AssetSupertype> AssetSupertypes { get; private set; } // "Tower", "Enemy", etc.

    public Analytics_AssetSuperclass(string[] supertypeNames)
    {
        AssetSupertypes = new List<Analytics_AssetSupertype>();

        foreach (string supertypeName in supertypeNames)
            AssetSupertypes.Add(new Analytics_AssetSupertype(supertypeName));
    }

    /// <summary>
    /// Adds a single asset
    /// </summary>
    public void AddAsset(string supertypeName, string subtypeName, int viewID, Vector3 pos)
    {
        var asset = FindSubtypeByName(supertypeName, subtypeName);

        asset.AddAsset(supertypeName, viewID, pos);
    }

    /// <summary>
    /// Returns a single Asset
    /// </summary>
    public Analytics_Asset FindAsset(string supertypeName, string subtypeName, int viewID)
    {
        var asset = FindSubtypeByName(supertypeName, subtypeName);

        return asset.FindAssetByViewID(viewID);
    }

    /// <summary>
    /// Returns subtype class (w/error checking)
    /// </summary>
    public Analytics_AssetSubtype FindSubtypeByName(string supertypeName, string subtypeName)
    {
        Analytics_AssetSupertype supertype = FindSupertypeByName(supertypeName);

        try 
        {
            DoesSubtypeExist(supertype, subtypeName);
        }
        catch (Exception e)
        {
            LogError("" + e);
            supertype.AddAssetSubtype(subtypeName);
        }

        return supertype.FindSubtypeByName(subtypeName);
    }

    /// <summary>
    /// Finds and returns the Supertype in a List by its name
    /// </summary>
    public Analytics_AssetSupertype FindSupertypeByName(string superTypeName)
    {
        return AssetSupertypes.Find(x => x.SupertypeName == superTypeName);
    }

    /// <summary>
    /// Throws error if the tower super type doesn't exist
    /// </summary>
    public void DoesSubtypeExist(Analytics_AssetSupertype supertype, string subtypeName)
    {
        if (!supertype.AssetSubtypes.Exists(x => x.SubtypeName == subtypeName))
                throw new ArgumentException(subtypeName + " is not an available subtype", "AssetSubtypes");
    }

    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[Analytics_AssetSuperclass] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[Analytics_AssetSuperclass] " + message);
    }
    #endregion MessageHandling
}

/// <summary>
/// Holds a List of the available Subtypes ("EMP", "Light Speeder", etc.) in a Supertype
/// Owner: John Fitzgerald
/// </summary>
public class Analytics_AssetSupertype
{
    public bool ShowDebugLogs = true;

    public string SupertypeName { get; private set; }                       // "Tower", "Enemy", etc.

    public List<Analytics_AssetSubtype> AssetSubtypes { get; private set; } // "EMP", "Light Speeder", etc.

    public Analytics_AssetSupertype(string supertypeName)
    {
        AssetSubtypes = new List<Analytics_AssetSubtype>();

        SupertypeName = supertypeName;
    }

    /// <summary>
    /// Add an asset subtype to the List
    /// </summary>
    public void AddAssetSubtype(string subtypeName)
    {
        AssetSubtypes.Add(new Analytics_AssetSubtype(subtypeName));
    }

    /// <summary>
    /// Finds and returns the Supertype in a List by its name
    /// </summary>
    public Analytics_AssetSubtype FindSubtypeByName(string subTypeName)
    {
        return AssetSubtypes.Find(x => x.SubtypeName == subTypeName);
    }

    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[Analytics_AssetSupertype] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[Analytics_AssetSupertype] " + message);
    }
    #endregion MessageHandling
}

/// <summary>
/// Holds a List of the individual Assets in a Subtype
/// Owner: John Fitzgerald
/// </summary>
public class Analytics_AssetSubtype
{
    public bool ShowDebugLogs = true;
    public bool IsAnalyticsSet {get; private set; }           // Determine if the Assets' analytics are ready to be sent to the server

    public string SubtypeName { get; private set; }           // The generic name of the asset (e.g. "Emp Tower" or "Light Speeder")

    public int NumberCreated { get; private set; }            // Number of this type of asset have been built in the level
    public int NumberDead { get; private set; }               // Number of this type fo asset that have been killed
    public float TotalLifeSpan {get; private set; }           // Lifespan sum for all assets
    public float TotalLifeSpanOfDead { get; private set; }    // Average Lifespan for all dead assets of this type
    public float ActivePercent { get; private set; }          // Percentage of time that asset Sub Type has been active

    public float TotalBallisticTaken { get; private set; }    // Ballstic Damage that the subtype has taken
    public float TotalThraceiumTaken { get; private set; }    // Thraceium Damage that the subtype has taken
    public float TotalBallisticDealt { get; private set; }    // Ballistic Damage that the subtype has dealt
    public float TotalThraceiumDealt { get; private set; }    // Thraceium Damage that the subtype has dealt
    public float TotalDPS { get; private set; }               // Total DPS that the subtype has dealt
    public float AvgDPS { get; private set; }                 // Avg DPS that the subtype has dealt

    public List<Analytics_Asset> Assets { get; private set; }  // Catalog individual asset sub types to be tracked in the level

    public Analytics_AssetSubtype(string displayName)
    {
        IsAnalyticsSet = false;
        SubtypeName = displayName;
        NumberCreated = 0;
        NumberDead = 0;
        TotalLifeSpan = 0;
        TotalLifeSpanOfDead = 0;
        
        TotalBallisticTaken = 0;
        TotalThraceiumTaken = 0;
        TotalBallisticDealt = 0;
        TotalThraceiumDealt = 0;
        TotalDPS = 0;
        AvgDPS = 0;

        Assets = new List<Analytics_Asset>();
    }

    /// <summary>
    /// Sets each asset's analytics before sending information to server
    /// </summary>
    private void SetAnalytics()
    {
        // Pull information from each individual asset
        foreach (Analytics_Asset asset in Assets)
        {
            NumberCreated++;
            TotalLifeSpan += asset.LifeSpan;
            TotalBallisticTaken += asset.DamageTaken_Ballistic;
            TotalThraceiumTaken += asset.DamageTaken_Thraceium;
            TotalBallisticDealt += asset.DamageDealt_Ballistic;
            TotalThraceiumDealt += asset.DamageDealt_Thraceium;
            TotalDPS += asset.DamageDealt_Ballistic + asset.DamageDealt_Thraceium;

            if (asset.IsDead)
            {
                NumberDead++;
                TotalLifeSpanOfDead += asset.LifeSpan;
            }
        }

        // TotalDPS is the total accrued damage divided by the total lifespan of all assets.
        TotalDPS /= TotalLifeSpan;
        // AvgDPS is the total damage per second divided by the number of assets created.
        AvgDPS = TotalDPS / NumberCreated;

        IsAnalyticsSet = true;
    }

    /// <summary>
    /// Add an asset to the list and increment the number of assets created
    /// </summary>
    public void AddAsset(string assetSupertype, int viewID, Vector3 assetOrigin)
    {
        Assets.Add(new Analytics_Asset(viewID, assetSupertype, SubtypeName, assetOrigin));
        NumberCreated++;
    }

    /// <summary>
    /// Find an asset by its unique View ID
    /// </summary>
    public Analytics_Asset FindAssetByViewID(int viewID)
    {
        return Assets.Find(x => x.ViewID == viewID);
    }

    /// <summary>
    /// Get number of created assets of this type
    /// </summary>
    public int GetNumberCreated()
    {
        if (!IsAnalyticsSet)
            SetAnalytics();

        return NumberCreated;
    }

    /// <summary>
    /// Get number of dead assets of this type
    /// </summary>
    public int GetNumberDead()
    {
        if (!IsAnalyticsSet)
            SetAnalytics();

        return NumberDead;
    }

    /// <summary>
    /// Get average lifespan of all of the dead tracked assets of this type
    /// </summary>
    public float GetTotalLifeSpanDead()
    {
        if (!IsAnalyticsSet)
            SetAnalytics();

        return TotalLifeSpanOfDead;
    }

    /// <summary>
    /// Get total amount of all the assets' DPS
    /// </summary>
    public float GetTotalDPS()
    {
        if (!IsAnalyticsSet)
            SetAnalytics();

        return TotalDPS;
    }
    
    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[Analytics_AssetSubtype] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[Analytics_AssetSubtype] " + message);
    }
    #endregion MessageHandling
}