using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TowerLoadoutButton : MonoBehaviour
{
	public bool ShowDebugLogs = false;

	public bool Active = false;
	public GameObject BackgroundImage;
	public GameObject TowerText;
	public GameObject PrevButton;
	public GameObject NextButton;

	public TowerData SelectedTowerData;

	private List<TowerData> AvailableTowerList;
	
	#region INITIALIZATION

	/// <summary>
	/// Sets the TowerData for the LoadoutButton
	/// </summary>
	/// <param name="towerData">TowerData used to set the LoadoutButton</param>
	public void SetTowerData(int selectedIndex, List<TowerData> availableTowers = null)
	{
		string towerDescription = "";

		// Set/Reset the available towers if there's a list
		if(availableTowers != null)
			AvailableTowerList = availableTowers;

		Active = true;
		SelectedTowerData = AvailableTowerList[selectedIndex];
		
		// Set background image based on level
		BackgroundImage.SetActive(true);
		BackgroundImage.GetComponent<Image>().sprite = Resources.Load("GUI/" + SelectedTowerData.PrefabName + "_Mug", typeof(Sprite)) as Sprite;

		int stringIndex = SelectedTowerData.DisplayName.IndexOf(" Tower");
		towerDescription = stringIndex != -1 ? SelectedTowerData.DisplayName.Remove(stringIndex) : SelectedTowerData.DisplayName;

		TowerText.GetComponent<Text>().text = towerDescription + "\n($" + SelectedTowerData.InstallCost.ToString() + ")";

		PrevButton.gameObject.SetActive(true);
		NextButton.gameObject.SetActive(true);
	}

	/// <summary>
	/// Sets the button as inactive. The player cannot choose a TowerLoadout from this button
	/// </summary>
	public void SetButtonInactive()
	{
		Active = false;
		BackgroundImage.SetActive(false);

		TowerText.GetComponent<Text>().text = "Tower Loadout Unavailable";

		PrevButton.gameObject.SetActive(false);
		NextButton.gameObject.SetActive(false);
	}
	#endregion

	/// <summary>
	/// Gets the next tower in a list of available towers based on the TowerID of the currently selected tower
	/// </summary>
	/// <returns>TowerData for the next Tower</returns>
	/// <param name="currentTowerID">ID of the currently selected Tower.</param>
	private int GetNextTowerID()
	{
		// Determines the next tower index based on the current TowerID
		int index = AvailableTowerList.FindIndex(x => x.TowerID == SelectedTowerData.TowerID) + 1;

		// Allow the list to loop from the end to the beginning
		if(index >= AvailableTowerList.Count)
			index = 0;

		return index;
	}

	/// <summary>
	/// Gets the previous tower in a list of available towers based on the TowerID of the currently selected tower
	/// </summary>
	/// <returns>TowerData for the previous Tower</returns>
	/// <param name="currentTowerID">ID of the currently selected Tower.</param>
	private int GetPrevTowerID()
	{
		// Determines the previous tower index based on the current TowerID
		int index = AvailableTowerList.FindIndex(x => x.TowerID == SelectedTowerData.TowerID) - 1;

		// Allows the list to loop from the beginning to the end
		if(index < 0)
			index = AvailableTowerList.Count - 1;

		return index;
	}

	#region EVENTS
	/// <summary>
	/// Click Event Handler called when player clicks the Next Tower button
	/// </summary>
	public void NextTower_Click()
	{
		SetTowerData(GetNextTowerID());
	}

	/// <summary>
	/// Click Event Handler called when player clicks the Prev Tower button
	/// </summary>
	public void PrevTower_Click()
	{
		SetTowerData(GetPrevTowerID());
	}
	#endregion

	#region MESSAGE HANDLING
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[TowerLoadoutButton] " + message);
	}
	
	protected void LogError(string message)
	{
		Debug.LogError("[TowerLoadoutButton] " + message);
	}
	#endregion
}
