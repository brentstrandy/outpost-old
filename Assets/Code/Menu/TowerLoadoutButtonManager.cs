using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TowerLoadoutButtonManager : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	public List<TowerLoadoutButton> TowerLoadoutButtonList;
	public List<TowerData> AvailableTowerList;

	public void OnEnable()
	{

	}

	/// <summary>
	/// Set the TowerData details for each Tower Loadout Button
	/// </summary>
	private void SetTowerButtonData()
	{
		int index = 0;

		// Run through and set each of the Tower Loadout Buttons
		foreach(TowerLoadoutButton tlb in TowerLoadoutButtonList)
		{
			// Only activate enough Tower Loadout Buttons if there are available Towers
			if(AvailableTowerList.Count > index)
				tlb.SetTowerData(index, AvailableTowerList);
			else
				tlb.SetButtonInactive();

			index++;
		}
	}

	/// <summary>
	/// Creates a list of TowerData based on the Level
	/// </summary>
	/// <param name="levelData">LevelData of the currently selected level</param>
	public void UpdateAvailableTowers(LevelData levelData)
	{
		AvailableTowerList.Clear();

		// Get a list of every tower the player can choose from
		foreach (TowerData towerData in GameDataManager.Instance.TowerDataManager.DataList.Where(x => PlayerManager.Instance.CurPlayer.TowerProgressDataManager.DataList.Any(y => x.TowerID == y.TowerID)).ToList())
		{
			// Determine if the tower is available for the chosen level
			if (levelData.AvailableTowers == "ALL" || levelData.AvailableTowers.Contains(towerData.DisplayName))
			{
				AvailableTowerList.Add(towerData);
			}
		}

		SetTowerButtonData();
	}

	/// <summary>
	/// Gets the TowerData for all currently selected towers
	/// </summary>
	/// <returns>A List of TowerData.</returns>
	public List<TowerData> GetSelectedTowerList()
	{
		List<TowerData> tempList = new List<TowerData>();

		// Run through each button to generate a list of all chosen Towers
		foreach(TowerLoadoutButton tlb in TowerLoadoutButtonList)
		{
			if(tlb.Active)
				tempList.Add(tlb.SelectedTowerData);
		}

		return tempList;
	}

	#region MessageHandling

	protected void Log(string message)
	{
		if (ShowDebugLogs)
			Debug.Log("[TowerLoadoutButtonManager] " + message);
	}

	protected void LogError(string message)
	{
		Debug.LogError("[TowerLoadoutButtonManager] " + message);
	}

	#endregion MessageHandling
}
