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

    // The generic name of the asset (e.g. "Emp Tower" or "Light Speeder")
    public string DisplayName;

    // How many of this type of asset have been built in the level
    private int NumberBuilt;
    // A List containing all the instances of the asset within the specified level
    private List<Analytics_Asset> Assets;

    public Analytics_TrackedAssets(string displayName)
    {
        DisplayName = displayName;
        NumberBuilt = 0;
        Assets = new List<Analytics_Asset>();
    }

    /// <summary>
    /// Add an asset to the list
    /// </summary>
    public void AddAsset(int viewID, string assetType, Vector2 assetOrigin)
    {
        Assets.Add(new Analytics_Asset(viewID, assetType, assetOrigin));
        NumberBuilt++;
    }

    /// <summary>
    /// 
    /// </summary>
    public Analytics_Asset FindByViewID(int viewID)
    {
        return Assets.Find(x => x.ViewID == viewID);
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