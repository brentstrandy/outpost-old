using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TowerLoadoutButtonManager : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	public List<TowerLoadoutButton> TowerLoadoutButtonList;
	public List<TowerData> AvailableTowerList;

	// Use this for initialization
	void Start ()
	{
		// Initialize the tower buttons by giving them all a reference to this Button Manager
		// This will allow the button manager to tell the button the next/prev available tower
		foreach(TowerLoadoutButton tlb in TowerLoadoutButtonList)
		{
			tlb.Init(this);
		}
	}

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
				tlb.SetTowerData(AvailableTowerList[index]);
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
	/// Gets the next tower in a list of available towers based on the TowerID of the currently selected tower
	/// </summary>
	/// <returns>TowerData for the next Tower</returns>
	/// <param name="currentTowerID">ID of the currently selected Tower.</param>
	public TowerData GetNextTower(int currentTowerID)
	{
		// Determines the next tower index based on the current TowerID
		int index = AvailableTowerList.FindIndex(x => x.TowerID == currentTowerID) + 1;

		// Allow the list to loop from the end to the beginning
		if(index >= AvailableTowerList.Count)
			index = 0;

		return AvailableTowerList[index];
	}

	/// <summary>
	/// Gets the previous tower in a list of available towers based on the TowerID of the currently selected tower
	/// </summary>
	/// <returns>TowerData for the previous Tower</returns>
	/// <param name="currentTowerID">ID of the currently selected Tower.</param>
	public TowerData GetPrevTower(int currentTowerID)
	{
		// Determines the previous tower index based on the current TowerID
		int index = AvailableTowerList.FindIndex(x => x.TowerID == currentTowerID) - 1;

		// Allows the list to loop from the beginning to the end
		if(index < 0)
			index = AvailableTowerList.Count - 1;

		return AvailableTowerList[index];
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
