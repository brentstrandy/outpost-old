using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class Armory_Menu : MonoBehaviour
{
    public bool ShowDebugLogs = true;

	public Dropdown TowerDropdown;
	public Dropdown AnimationsDropdown;

	private string SelectedTowerName;
	private GameObject SelectedTower;

    private void OnEnable()
    {
		TowerDropdown.options.Clear();

		// Populate TowerDropdown with current list of towers
		TowerDropdown.AddOptions(GameDataManager.Instance.TowerDataManager.DataList.Select(x => x.DisplayName).ToList());
    }

    private void OnDisable()
    {
		
    }

    #region OnClick

	public void Tower_Click()
	{
		// Only load/unload tower if it is a new tower
		if(TowerDropdown.captionText.text != SelectedTowerName)
		{
			SelectedTowerName = TowerDropdown.captionText.text;
			Destroy(SelectedTower);
			// Get the tower's prefab based on its name
			string prefabName = GameDataManager.Instance.FindTowerPrefabByDisplayName(SelectedTowerName);
			// Create a "Look" quaternion that considers the Z axis to be "up" and that faces away from the base
			var rotation = Quaternion.LookRotation(new Vector3(100, 100, 0), new Vector3(0.0f, 0.0f, -1.0f));
			// Instantiate the Armory version of the tower
			SelectedTower = Instantiate(Resources.Load("Towers/Armory/" + prefabName + "_Armory"), new Vector3(-4.39f, 7.51f, -1.09f), rotation) as GameObject;
		}
	}

    /// <summary>
    /// Used by the GUI system to go leave the room when the Back button is pressed
    /// </summary>
    public void Back_Click()
	{
		MenuManager.Instance.ReturnToMainMenu();
    }

    #endregion OnClick

    #region Events


    #endregion Events

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[Armory_Menu] " + message);
    }

    protected void LogError(string message)
    {
		Debug.LogError("[Armory_Menu] " + message);
    }

    #endregion MessageHandling
}