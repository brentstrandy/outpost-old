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

	public string GetTowerIDs()
	{
		string list = "";

		foreach(TowerData td in Towers)
			list += td.TowerID.ToString() + ",";

		if(list != "")
			list = list.Substring(0, list.Length - 1);

		return list;
	}
}