using System.Collections.Generic;

/// <summary>
/// Defines player-specific aspects of a game.
/// </summary>
public class LoadOut
{
    public List<TowerData> Towers { get; private set; }

    public LoadOut()
    {
    }

    public LoadOut(List<TowerData> towers)
    {
        Towers = towers;
    }

    public void SetTowerList(List<TowerData> towers)
    {
        Towers = towers;
    }
}