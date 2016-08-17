using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EditorLevelEditorMenu : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	private LevelData CurrentLevelData;

	public RectTransform MenuBarScreenPanel;

	// References to Load Screen GUI objects
	public RectTransform LoadScreenPanel;
	public Dropdown LoadLevelDropdown;
	public Dropdown CreateNewLevelDropdown;

	// Reference to Details GUI objects
	public InputField DisplayNameTextbox;
	public InputField DescriptionTextbox;
	public InputField AvailableTowersTextbox;
	public InputField EnemySpawnTextbox;
	public InputField NotificationTextbox;
	public InputField MinPlayersTextbox;
	public InputField MaxPlayersTextbox;
	public InputField TowerSlotsTextbox;
	public InputField StartingMoneyTextbox;
	public InputField AuthorTextbox;
	public Text DateUpdatedLabel;

	private void OnEnable()
	{
		PopulateLevelDropdowns();
	}

	private void OnDisable()
	{

	}

	public void OnLoadLevelSelected()
	{
		string levelTitle = LoadLevelDropdown.captionText.text;

		if(levelTitle != "")
		{
			LoadLevelDropdown.interactable = false;
			CreateNewLevelDropdown.interactable = false;

			CurrentLevelData = EditorMenuManager.Instance.LoadLevel(levelTitle);

			StartCoroutine(LoadingDelay());

			PopulateDetails();
		}
	}

	public void OnCreateNewLevelSelected()
	{
		string levelTitle = CreateNewLevelDropdown.captionText.text;

		// TODO: Create a new level
		if(levelTitle != "")
		{
			CreateNewLevelDropdown.interactable = false;
			LoadLevelDropdown.interactable = false;

			if(levelTitle != "Create New Level...")
				CurrentLevelData = EditorMenuManager.Instance.LoadLevel(levelTitle);
			else
				CurrentLevelData = new LevelData();

			StartCoroutine(LoadingDelay());

			PopulateDetails();
		}
	}

	public void PopulateLevelDropdowns()
	{
		// TODO: Get data from Josh instead of hardcoded
		// Gather a list of all level titles
		List<string> levelTitles = new List<string>{"Test Level", "Something New", "Desperation", "PMCC1171701", "Colossus", "The Grind"};

		// Enable Dropdowns
		LoadLevelDropdown.interactable = true;
		CreateNewLevelDropdown.interactable = true;

		// Clear dropdowns
		LoadLevelDropdown.ClearOptions();
		CreateNewLevelDropdown.ClearOptions();
		// Add blank options at top of list to force user to select an option
		LoadLevelDropdown.options.Add(new Dropdown.OptionData(""));
		CreateNewLevelDropdown.options.Add(new Dropdown.OptionData(""));

		// Create New Level has a default option always present
		CreateNewLevelDropdown.options.Add(new Dropdown.OptionData("Create New Level..."));

		// Populate dropdowns
		foreach(string title in levelTitles)
		{
			LoadLevelDropdown.options.Add(new Dropdown.OptionData(title));
			CreateNewLevelDropdown.options.Add(new Dropdown.OptionData(title));
		}

		LoadLevelDropdown.value = 0;
		CreateNewLevelDropdown.value = 0;

		LoadLevelDropdown.captionText.text = "Select Level...";
		CreateNewLevelDropdown.captionText.text = "Select Template...";
	}

	private void PopulateDetails()
	{
		DisplayNameTextbox.text = CurrentLevelData.DisplayName;
		DescriptionTextbox.text = CurrentLevelData.Description;
		AvailableTowersTextbox.text = CurrentLevelData.AvailableTowers;
		EnemySpawnTextbox.text = CurrentLevelData.EnemySpawnFilename;
		NotificationTextbox.text = CurrentLevelData.NotificationFilename;
		MinPlayersTextbox.text = CurrentLevelData.MinimumPlayers.ToString();
		MaxPlayersTextbox.text = CurrentLevelData.MaximumPlayers.ToString();
		TowerSlotsTextbox.text = CurrentLevelData.MaxTowersPerPlayer.ToString();
		StartingMoneyTextbox.text = CurrentLevelData.StartingMoney.ToString();
		// TODO: Pull author from LevelData
		AuthorTextbox.text = "<author>";
		// TODO: Pull Date Updated from LevelData
		DateUpdatedLabel.text = "<date/time>";
	}

	private IEnumerator LoadingDelay()
	{
		// Need to add a second so that GUI elements can clean themselves up.
		yield return new WaitForSeconds(1);

		// Load new menu
		LoadScreenPanel.gameObject.SetActive(false);
		MenuBarScreenPanel.gameObject.SetActive(true);
	}

	#region MessageHandling

	protected void Log(string message)
	{
		if (ShowDebugLogs)
			Debug.Log("[EditorLevelEditorMenu] " + message);
	}

	protected void LogError(string message)
	{
		Debug.LogError("[EditorLevelEditorMenu] " + message);
	}

	#endregion MessageHandling
}
