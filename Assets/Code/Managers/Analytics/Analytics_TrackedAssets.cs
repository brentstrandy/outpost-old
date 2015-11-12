using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

/// <summary>
/// A container (List) of "Analytics_Asset.cs"s to be tracked by AnalyticsManager.
/// Owner: John Fitzgerald
/// </summary>
public class Analytics_TrackedAssets
{
    public bool ShowDebugLogs = true;
    public bool IsAnalyticsSet {get; private set; }           // 

    public string DisplayName { get; private set; }           // The generic name of the asset (e.g. "Emp Tower" or "Light Speeder")

    public int NumberCreated { get; private set; }            // Number of this type of asset have been built in the level
    public int NumberDead { get; private set; }               // Number of this type fo asset that have been killed
    public float TotalLifeSpan {get; private set; }           // Lifespan sum for all assets
    public float TotalLifeSpanOfDead { get; private set; }    // Average Lifespan for all dead assets of this type
    public float ActivePercent { get; private set; }          // Percentage of time that asset Sub Type has been active

    public float TotalBallisticTaken { get; private set; }    // 
    public float TotalThraceiumTaken { get; private set; }    // 
    public float TotalBallisticDealt { get; private set; }    // 
    public float TotalThraceiumDealt { get; private set; }    // 
    public float TotalDPS { get; private set; }               // 
    public float AvgDPS { get; private set; }                 // 

    public List<Analytics_Asset> Assets { get; private set; }  // A List containing all the instances of the asset within the specified level

    // constructor
    public Analytics_TrackedAssets(string displayName)
    {
        IsAnalyticsSet = false;
        DisplayName = displayName;
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
    /// Add an asset to the list and increment the number of assets built
    /// </summary>
    public void AddAsset(int viewID, string assetSupertype, Vector2 assetOrigin)
    {
        Assets.Add(new Analytics_Asset(viewID, assetSupertype, DisplayName, assetOrigin));
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
    /// 
    /// </summary>
    private void SetAnalytics()
    {
        // 
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

        TotalDPS /= TotalLifeSpan;
        AvgDPS = TotalDPS / NumberCreated;
        ActivePercent = (ActivePercent / AnalyticsManager.Instance.GameLength) * 100;

        IsAnalyticsSet = true;
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
    public float GetTotalLifeSpanOfDead()
    {
        if (!IsAnalyticsSet)
            SetAnalytics();

        return TotalLifeSpanOfDead;
    }

    /// <summary>
    /// 
    /// </summary>
    public float GetTotalDPS()
    {
        if (!IsAnalyticsSet)
            SetAnalytics();

        return TotalDPS;
    }

    public float GetActivePercent()
    {
        if (!IsAnalyticsSet)
            SetAnalytics();

        return ActivePercent;
    }
    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[Analytics_TrackedAssets] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[Analytics_TrackedAssets] " + message);
    }
    #endregion MessageHandling
}