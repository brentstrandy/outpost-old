using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InGame_Menu : MonoBehaviour
{
    public bool ShowDebugLogs = true;

    public List<GameObject> TowerButtons;
	public Text MoneyText;
	public Text OutpostHealthText;
	public Text ScoreText;
	public Text KillsText;
    public Text TimeText;

    private bool FinishedLoadingLevel = false;

    private void OnEnable()
    {
        // Get the list of tower names in the loadout for the player
        List<TowerData> towerData = PlayerManager.Instance.GetGameLoadOutTowers();
        int index = 0;

        // Run through each button slot available to the player and associate it with
        // the tower name being used by the player during this game
        foreach (GameObject towerButton in TowerButtons)
        {
            // Check to see if there is a tower name in the loadout to use for this button. If there is no
            // tower name then simple disable/hide the button
            if (towerData.Count > index)
            {
                // td MUST be instantiated within the foreach loop because AddListener saves a reference and will only
                // use the last referenced TowerData variable for each of the buttons
                TowerData td = towerData[index];
                towerButton.SetActive(true);
                towerButton.GetComponentInChildren<Text>().text = "($" + towerData[index].InstallCost.ToString() + ")";
                towerButton.GetComponent<Button>().onClick.AddListener(() => Tower_Click(td));
				foreach(Image image in towerButton.GetComponentsInChildren<Image>())
				{
					if(image.name == "Image")
						image.sprite = Resources.Load("GUI/" + towerData[index].PrefabName + "_Mug", typeof(Sprite)) as Sprite;
				}
            }
            else
                towerButton.SetActive(false);

            index++;
        }

        // Add listeners for hotkeys
        InputManager.Instance.OnTowerHotKeyPressed += OnTowerHotKeyPressed;
		InputManager.Instance.OnShowMenuKeyPressed += OnShowMenuKeyPressed;
    }

    private void OnDisable()
    {
        FinishedLoadingLevel = false;
		if(InputManager.Instance)
		{
			InputManager.Instance.OnTowerHotKeyPressed -= OnTowerHotKeyPressed;
			InputManager.Instance.OnShowMenuKeyPressed -= OnShowMenuKeyPressed;
		}
    }

    public void Update()
    {
        // Only check for updates if the level has loaded and the game is currently running
        if (FinishedLoadingLevel)
        {
            if (GameManager.Instance.GameRunning)
            {
                // Display how much money the player current has
                MoneyText.text = "Money: " + Mathf.FloorToInt(PlayerManager.Instance.CurPlayer.Money).ToString();
                OutpostHealthText.text = "Health: " + GameManager.Instance.ObjMiningFacility.Health.ToString();
                ScoreText.text = "Score: " + PlayerManager.Instance.CurPlayer.Score.ToString();
				KillsText.text = "Kills: " + PlayerManager.Instance.CurPlayer.KillCount.ToString();
				TimeText.text = (Time.time - GameManager.Instance.LevelStartTime).ToString("F2");
            }
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        FinishedLoadingLevel = true;
    }

	#region EVENTS
    private void OnTowerHotKeyPressed(int towerIndex)
    {
        // Only perform the hotkey actions if there is a tower button to select
        if (TowerButtons.Count > towerIndex && towerIndex >= 0)
        {
            ExecuteEvents.Execute(TowerButtons[towerIndex], new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
        }
    }

	private void OnShowMenuKeyPressed()
	{
		// Only allow the player to access the in-game pause menu while the game is running
		// This means the player cannot view this menu while looking at the post-game stats
		if(GameManager.Instance.GameRunning)
			MenuManager.Instance.ToggleInGamePauseMenu();
	}
	#endregion

    #region OnClick

    public void Tower_Click(TowerData towerData)
    {
        // Inform the player a tower has been selected for placement
        PlayerInteractionManager.Instance.TowerSelectedForPlacement(towerData);
    }

    #endregion OnClick

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[InGame_Menu] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[InGame_Menu] " + message);
    }

    #endregion MessageHandling
}