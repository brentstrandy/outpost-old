using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InGame_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	public List<GameObject> TowerButtons;
	public GameObject MoneyText;
	public GameObject OutpostHealthText;
	public GameObject DirectionText;

	private bool FinishedLoadingLevel = false;
	
	private void OnEnable()
	{
		// Get the list of tower names in the loadout for the player
		List<TowerData> towerData = PlayerManager.Instance.GetGameLoadOutTowers();
		int index = 0;

		// Run through each button slot available to the player and associate it with
		// the tower name being used by the player during this game
		foreach(GameObject towerButton in TowerButtons)
		{
			// Check to see if there is a tower name in the loadout to use for this button. If there is no
			// tower name then simple disable/hide the button
			if(towerData.Count > index)
			{
				// td MUST be instantiated within the foreach loop because AddListener saves a reference and will only
				// use the last referenced TowerData variable for each of the buttons
				TowerData td = towerData[index];
				towerButton.SetActive(true);
				towerButton.GetComponentInChildren<Text>().text = towerData[index].DisplayName + " ($" + towerData[index].InstallCost.ToString() + ")";
				towerButton.GetComponent<Button>().onClick.AddListener(() => Tower_Click(td));
			}
			else
				towerButton.SetActive(false);

			index++;
		}

		// Add listeners for hotkeys
		InputManager.Instance.OnTowerHotKeyPressed += OnTowerHotKey;
	}

	private void OnDisable()
	{
		FinishedLoadingLevel = false;
	}

	public void Update()
	{
		// Only check for updates if the level has loaded and the game is currently running
		if(FinishedLoadingLevel)
		{
			if(GameManager.Instance.GameRunning)
			{
				// Display how much money the player current has
				MoneyText.GetComponent<Text>().text = "Money: " + Mathf.FloorToInt(PlayerManager.Instance.Money).ToString();
				OutpostHealthText.GetComponent<Text>().text = "Health: " + GameManager.Instance.ObjMiningFacility.Health.ToString();
			}
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		FinishedLoadingLevel = true;
	}

	private void OnTowerHotKey(int towerIndex)
	{
		// Only perform the hotkey actions if there is a tower button to select
		if(TowerButtons.Count > towerIndex && towerIndex >= 0)
		{
			ExecuteEvents.Execute(TowerButtons[towerIndex], new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
		}
	}

	#region OnClick
	public void Tower_Click(TowerData towerData)
	{
		// Inform the player a tower has been selected for placement
		PlayerManager.Instance.TowerSelectedForPlacement(towerData);
	}
	#endregion

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[InGame_Menu] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[InGame_Menu] " + message);
	}
	#endregion
}
