using UnityEngine;
using System.Collections;
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

	/// <summary>
	/// Reference to the Tower Loadout Button Manager. Used to determine the next and previous towers to choose from
	/// </summary>
	private TowerLoadoutButtonManager TowerLoadoutButtonMngr;
	
	#region INITIALIZATION

	/// <summary>
	/// Initialize the Tower Loadout Button
	/// </summary>
	/// <param name="tlbm">Reference to the TowerLoadoutButtonManager needed by the button</param>
	public void Init(TowerLoadoutButtonManager tlbm)
	{
		TowerLoadoutButtonMngr = tlbm;
	}

	/// <summary>
	/// Sets the TowerData for the LoadoutButton
	/// </summary>
	/// <param name="towerData">TowerData used to set the LoadoutButton</param>
	public void SetTowerData(TowerData towerData)
	{
		string towerDescription = "";

		Active = true;
		SelectedTowerData = towerData;
		
		// Set background image based on level
		BackgroundImage.SetActive(true);
		BackgroundImage.GetComponent<Image>().sprite = Resources.Load("GUI/" + towerData.PrefabName + "_Mug", typeof(Sprite)) as Sprite;

		int stringIndex = towerData.DisplayName.IndexOf(" Tower");
		towerDescription = stringIndex != -1 ? towerData.DisplayName.Remove(stringIndex) : towerData.DisplayName;

		TowerText.GetComponent<Text>().text = towerDescription + "\n($" + towerData.InstallCost.ToString() + ")";

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

	#region EVENTS
	/// <summary>
	/// Click Event Handler called when player clicks the Next Tower button
	/// </summary>
	public void NextTower_Click()
	{
		SetTowerData(TowerLoadoutButtonMngr.GetNextTower(SelectedTowerData.TowerID));
	}

	/// <summary>
	/// Click Event Handler called when player clicks the Prev Tower button
	/// </summary>
	public void PrevTower_Click()
	{
		SetTowerData(TowerLoadoutButtonMngr.GetPrevTower(SelectedTowerData.TowerID));
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
