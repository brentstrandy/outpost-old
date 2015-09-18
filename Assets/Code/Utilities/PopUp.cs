using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Helps create the prefab for the PopUpController.cs class.
/// Control the initial position of the numbers here.
/// Owner: John Fitzgerald
/// </summary>
public class PopUp<T>
{
    public bool ShowDebugLogs = true;

    private string popUpLocation = "Utilities/PopUp";

    private GameObject PopUpDisplay;
    private GameObject ObjectOfEffect;

    private float ValueToDisplay;
    private float DisplayLength;

    private Vector3 PopUp_Pos;
    private Vector3 Parent_Pos;

    public PopUp(GameObject objectOfEffect, float valueToDisplay, float displayLength)
    {
        ObjectOfEffect = objectOfEffect;
        ValueToDisplay = valueToDisplay;
        DisplayLength = displayLength;
        Parent_Pos = objectOfEffect.gameObject.transform.position;

        SetPopUp_Pos();
        InstantiatePopUpPrefab();
        InitializePopUpGO();
    }

    private void SetPopUp_Pos()
    {
        // PopUp position is random within a range
        float[] pos = {0, 0, 0};
        for (int i = 0; i < 2; i++)
            pos[i] = UnityEngine.Random.Range(-0.2f, 0.2f);

        PopUp_Pos = new Vector3(Parent_Pos.x + pos[0], Parent_Pos.y + pos[1], Parent_Pos.z + pos[2] - 2f);
    }

    private void InstantiatePopUpPrefab()
    {
        // Instantiate the PopUp prefab and add the PopUpController script to it
        PopUpDisplay = ScriptableObject.Instantiate(Resources.Load(popUpLocation), PopUp_Pos, Camera.main.transform.rotation) as GameObject;

        //PopUpDisplay = Instantiate(Resources.Load(popUpLocation)) as GameObject;
        //PopUpDisplay.transform.position = PopUp_Pos;
        //PopUpDisplay.transform.rotation = Camera.main.transform.rotation;
    }

    private void InitializePopUpGO()
    {
        PopUpController popUpController_Script = PopUpDisplay.AddComponent<PopUpController>();

        popUpController_Script.InitializePopUp(PopUpDisplay, ObjectOfEffect.gameObject, DisplayLength, ValueToDisplay, typeof(T));
    }

    #region MessageHandling
    private void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[PopUp<T>] " + message);
    }

    private void LogError(string message)
    {
        Debug.LogError("[PopUp<T>] " + message);
    }

    private void LogWarning(string message)
    {
        if (ShowDebugLogs)
            Debug.LogWarning("[PopUp<T>] " + message);
    }
    #endregion
}