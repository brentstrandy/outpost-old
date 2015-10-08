using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a reference to all currently active Enemies
/// Created By: Brent Strandy
/// </summary>
public class EnemyManager
{
    public bool ShowDebugLogs = true;

    private List<Enemy> ActiveEnemyList;

    public EnemyManager()
    {
        ActiveEnemyList = new List<Enemy>();
    }

    public int ActiveEnemyCount()
    {
        return ActiveEnemyList.Count;
    }

    public void AddActiveEnemy(Enemy enemy)
    {
        ActiveEnemyList.Add(enemy);
    }

    public void RemoveActiveEnemy(Enemy enemy)
    {
        ActiveEnemyList.Remove(enemy);
    }

    public Enemy FindEnemyByID(int viewID)
    {
        return ActiveEnemyList.Find(x => x.NetworkViewID == viewID);
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[EnemyManager] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[EnemyManager] " + message);
    }

    #endregion MessageHandling
}