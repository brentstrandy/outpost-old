using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

/// <summary>
/// A collection of assets to be tracked by AnalyticsManager
/// Owner: John Fitzgerald
/// </summary>
public class Analytics_TrackedAssets// : ScriptableObject
{
    private string DisplayName;
    private List<Analytics_Asset> Assets;
    public int NumberBuilt;

    public Analytics_TrackedAssets(string displayName)
    {
        DisplayName = displayName;
        NumberBuilt = 0;
    }

    /// <summary>
    /// Add an enemy to the list
    /// </summary>
    public void AddEnemy(int viewID, string assetType)
    {
        Assets.Add(new Analytics_Asset(viewID, assetType));
        NumberBuilt++;
    }

    /// <summary>
    /// Add a tower to the list (includes the tower's origin Vector3)
    /// </summary>
    public void AddTower(int viewID, string assetType, Vector3 assetOrigin)
    {
        Assets.Add(new Analytics_Asset(viewID, assetType, assetOrigin));
        NumberBuilt++;
    }
}