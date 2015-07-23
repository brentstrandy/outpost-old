using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TowerButton : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	
	private TowerData TowerAttributes;
	
	public GameObject BackgroundImage;
	public GameObject TowerText;
	public GameObject SelectedImage;
	
	public bool Selected { get; private set; }
	
	#region INITIALIZATION
	public void SetTowerData(TowerData towerData)
	{
		string towerDescription = "";
		
		TowerAttributes = towerData;
		
		// Set background image based on level
		BackgroundImage.GetComponent<Image>().sprite = Resources.Load("GUI/" + towerData.PrefabName + "_Mug", typeof(Sprite)) as Sprite;

		int stringIndex = towerData.DisplayName.IndexOf(" Tower");
		towerDescription = stringIndex != -1 ? towerData.DisplayName.Remove(stringIndex) : towerData.DisplayName;
		
		TowerText.GetComponent<Text>().text = towerDescription + "\n($" + towerData.InstallCost.ToString() + ")";
	}
	#endregion
	
	public void SelectTower()
	{
		Selected = true;
		
		SelectedImage.SetActive(true);
	}
	
	public void UnselectTower()
	{
		Selected = false;
		
		SelectedImage.SetActive(false);
	}

	public void ClickTower()
	{
		if(Selected)
			UnselectTower();
		else
			SelectTower();
	}

	#region MESSAGE HANDLING
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[TowerButton] " + message);
	}
	
	protected void LogError(string message)
	{
		Debug.LogError("[TowerButton] " + message);
	}
	#endregion
}
