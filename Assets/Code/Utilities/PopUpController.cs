using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Displays heal and damage numbers above GameObjects.
/// Owner: John Fitzgerald
/// </summary>
public class PopUpController : MonoBehaviour
{
    public bool ShowDebugLogs = true;

    private Camera MainCamera;

    GameObject ObjectOfEffect;
    GameObject PopUp;

    public int AmountToDisplay;
    public float DisplayLength;
    public float DeleteTime;

    public float XAxisOffSet;
    public float YAxisOffSet;
    public float ZAxisOffSet;
    public float FallDirection;

    public Vector3 PopUp_Position;
    public TextMesh _TextMesh;

    void Awake()
    {
        MainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // Change in y-axis
        //YAxisOffSet -= 0f;
        // Change in x-axis (TODO -- (FITZGERALD) needs tweaking as enemies are rarely ever static)
        //XAxisOffSet = FallDirection == 0 ? 0.1f : -0.1f;
        ZAxisOffSet += 0.08f;
        // Numbers will fall "down" and to the side
        PopUp.transform.position = new Vector3(XAxisOffSet, YAxisOffSet, ZAxisOffSet);

        if (DeleteTime <= Time.time || ObjectOfEffect == null)
            Destroy(PopUp);
    }

    public void InitializePopUp(GameObject popUp, GameObject objectOfEffect, float displayLength, float popUpValue, Type type)
    {
        popUp.transform.parent = objectOfEffect.transform;

        PopUp = popUp;
        ObjectOfEffect = objectOfEffect;

        AmountToDisplay = (int)popUpValue;
        DisplayLength = displayLength;
        DeleteTime = Time.time + DisplayLength;

        XAxisOffSet = popUp.transform.position.x;
        YAxisOffSet = popUp.transform.position.y;
        ZAxisOffSet = popUp.transform.position.z;
        FallDirection = UnityEngine.Random.Range(0, 1);

        PopUp_Position = popUp.transform.position;
        _TextMesh = PopUp.GetComponentInChildren<TextMesh>();

        Display(type);
        // Delete's the PopUp numbers after the alloted DisplayLength
        //DeletePopUp();
    }

    private void Display(Type type)
    {
        // A display function will be called depending on the Type's base class
        if (type == typeof(ThraceiumHealingTower))
            DisplayHealthGained();
        else if (type == typeof(Enemy) || type == typeof(Tower))
            DisplayDamageDealt();
        else
            LogError("Type is not correctly instantiated.");
    }

    private void DisplayDamageDealt()
    {
        _TextMesh.color = Color.red;
        _TextMesh.text = AmountToDisplay.ToString();
    }

    // TODO -- (FITZGERALD) make it so damage is shown on the camera of each player, regardless of the quadrant they're in
    private void DisplayDamageDealt_MiningFacility()
    {
        //_TextMesh.color = Color.red;
        //_TextMesh.text = AmountToDisplay.ToString();
    }

    private void DisplayHealthGained()
    {
        string amountToDisplay = "+" + AmountToDisplay.ToString();

        _TextMesh.color = Color.green;
        _TextMesh.text = amountToDisplay;
    }

    //private void DeletePopUp()
    //{
    //    Destroy(PopUp, DisplayLength);
    //}

    #region MessageHandling
    private void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[PopUpController] " + message);
    }

    private void LogError(string message)
    {
        Debug.LogError("[PopUpController] " + message);
    }

    private void LogWarning(string message)
    {
        if (ShowDebugLogs)
            Debug.LogWarning("[PopUpController] " + message);
    }
    #endregion
}