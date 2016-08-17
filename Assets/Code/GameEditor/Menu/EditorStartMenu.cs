using UnityEngine;
using System.Collections;

public class EditorStartMenu : MonoBehaviour 
{
	public bool ShowDebugLogs = true;
	private bool LevelDataLoaded, TowerDataLoaded, EnemyDataLoaded = false;

	private void OnEnable()
    {
    	StartCoroutine(WaitForGameLoad());
    }

    private void OnDisable()
    {
    	if(GameDataManager.Instance != null)
    	{
			GameDataManager.Instance.LevelDataManager.OnDataLoadSuccess -= OnLevelDataLoadedSuccess;
	        GameDataManager.Instance.LevelDataManager.OnDataLoadFailure -= OnDataLoadError;
	        GameDataManager.Instance.TowerDataManager.OnDataLoadSuccess -= OnTowerDataLoadedSuccess;
	        GameDataManager.Instance.TowerDataManager.OnDataLoadFailure -= OnDataLoadError;
	        GameDataManager.Instance.EnemyDataManager.OnDataLoadSuccess -= OnEnemyDataLoadedSuccess;
	        GameDataManager.Instance.EnemyDataManager.OnDataLoadFailure -= OnDataLoadError;
        }
    }

    private void OnLevelDataLoadedSuccess()
    {
        LevelDataLoaded = true;
        ValidateDataLoaded();
    }

	private void OnTowerDataLoadedSuccess()
	{
		TowerDataLoaded = true;
        ValidateDataLoaded();
	}

    private void OnEnemyDataLoadedSuccess()
    {
        EnemyDataLoaded = true;
        ValidateDataLoaded();
    }

    private void OnDataLoadError()
    {
        LogError("Error Downloading data from server");
    }

	private void ValidateDataLoaded()
	{
		// Transition scenes if 
		if(LevelDataLoaded && TowerDataLoaded && EnemyDataLoaded)
            EditorMenuManager.Instance.ShowMainMenuPanel();
			
	}

	private IEnumerator WaitForGameLoad()
	{
		// Pause on loading screen to allow the GameDataManager to instantiate - and user to see splash screen
		yield return new WaitForSeconds(2);
			
		GameDataManager.Instance.SwitchToServerData();

		GameDataManager.Instance.LevelDataManager.OnDataLoadSuccess += OnLevelDataLoadedSuccess;
        GameDataManager.Instance.LevelDataManager.OnDataLoadFailure += OnDataLoadError;
        GameDataManager.Instance.TowerDataManager.OnDataLoadSuccess += OnTowerDataLoadedSuccess;
        GameDataManager.Instance.TowerDataManager.OnDataLoadFailure += OnDataLoadError;
        GameDataManager.Instance.EnemyDataManager.OnDataLoadSuccess += OnEnemyDataLoadedSuccess;
        GameDataManager.Instance.EnemyDataManager.OnDataLoadFailure += OnDataLoadError;
	}

	#region MessageHandling

	protected void Log(string message)
	{
		if (ShowDebugLogs)
			Debug.Log("[EditorStartMenu] " + message);
	}

	protected void LogError(string message)
	{
		Debug.LogError("[EditorStartMenu] " + message);
	}

	#endregion MessageHandling
}
