using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public bool ShowDebugLogs = true;

    public GameObject BackgroundImage;
    public GameObject LevelText;
    public GameObject SelectedImage;

    public bool Selected { get; private set; }

    #region INITIALIZATION

    public void SetLevelData(LevelData levelData)
    {
        string levelDescription = "";

        // Set background image based on level
        BackgroundImage.GetComponent<Image>().sprite = Resources.Load("GUI/" + levelData.SceneName + "_Mug", typeof(Sprite)) as Sprite;

        // Set level details
        if (levelData.MinimumPlayers == 1 && levelData.MaximumPlayers == 1)
            levelDescription = levelData.DisplayName + "\n[1 Player]";
        else if (levelData.MinimumPlayers == levelData.MaximumPlayers)
            levelDescription = levelData.DisplayName + "\n[" + levelData.MinimumPlayers + " Players]";
        else
            levelDescription = levelData.DisplayName + "\n[" + levelData.MinimumPlayers + " - " + levelData.MaximumPlayers + " Players]";

        LevelProgressData lpd = PlayerManager.Instance.LevelProgressDataManager.DataList.Find(x => x.LevelID == levelData.LevelID);
        if (lpd != null)
            levelDescription += "\n[Score: " + lpd.Score.ToString() + "]";
        else
            levelDescription += "\n[Not Played]";

        LevelText.GetComponent<Text>().text = levelDescription;
    }

    #endregion INITIALIZATION

    public void SelectLevel()
    {
        Selected = true;

        SelectedImage.SetActive(true);
    }

    public void UnselectLevel()
    {
        Selected = false;

        SelectedImage.SetActive(false);
    }

    #region MESSAGE HANDLING

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[LevelButton] " + message);
    }

    protected void LogError(string message)
    {
        Debug.LogError("[LevelButton] " + message);
    }

    #endregion MESSAGE HANDLING
}