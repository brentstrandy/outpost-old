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

    public string DisplayName;            // The generic name of the asset (e.g. "Emp Tower" or "Light Speeder")

    private int NumberBuilt;              // Number of this type of asset have been built in the level
    private int NumberDead;               // Number of this type fo asset that have been killed
    private float AverageLifeSpan;        // Average Lifespan for all dead assets of this type

    public List<Analytics_Asset> Assets { get; private set; } // A List containing all the instances of the asset within the specified level

    // constructor
    public Analytics_TrackedAssets(string displayName)
    {
        DisplayName = displayName;
        NumberBuilt = 0;
        NumberDead = 0;
        AverageLifeSpan = 0;
        Assets = new List<Analytics_Asset>();
    }

    /// <summary>
    /// Add an asset to the list and increment the number of assets built
    /// </summary>
    public void AddAsset(int viewID, string assetSupertype, Vector2 assetOrigin)
    {
        Assets.Add(new Analytics_Asset(viewID, assetSupertype, DisplayName, assetOrigin));
        NumberBuilt++;
    }

    /// <summary>
    /// Find an asset by its unique View ID
    /// </summary>
    public Analytics_Asset FindAssetByViewID(int viewID)
    {
        return Assets.Find(x => x.ViewID == viewID);
    }

    /// <summary>
    /// Get average lifespan of all the dead tracked assets of this type
    /// </summary>
    public float GetAverageLifespan()
    {
        // Locate all currently dead assets and sum their lifespan
        foreach (Analytics_Asset asset in Assets)
        {
            if (asset.IsDead == true)
            {
                NumberDead++;
                AverageLifeSpan += asset.LifeSpan;
            }
        }

        AverageLifeSpan = AverageLifeSpan / NumberDead;

        return AverageLifeSpan;
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