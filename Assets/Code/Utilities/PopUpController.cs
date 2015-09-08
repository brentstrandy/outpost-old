using UnityEngine;
using System;
using System.Collections;

// TODO -- (FITZGERALD) Cannot add a generic class to GameObjects. Make PopUpController non-hardcoded dynamic (like XMLParser).
public class PopUpController : MonoBehaviour
{
    public bool ShowDebugLogs = true;

    private Camera MainCamera;

    GameObject ObjectOfEffect;
    GameObject PopUp;

    public int AmountToDisplay;
    //public float DisplayLength;
    public float TimeToDelete;
    public float XAxisOffSet;
    public float YAxisOffSet;
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
            PopUp.transform.rotation = MainCamera.transform.rotation;

            // Change in y-axis
            YAxisOffSet -= 0.08f;
            // Change in x-axis (needs tweaking as enemies are rarely ever static)
            XAxisOffSet = FallDirection == 0 ? 0.1f : -0.4f;
            
            PopUp.transform.position = new Vector3(ObjectOfEffect.transform.position.x, YAxisOffSet, ObjectOfEffect.transform.position.z);

            if (TimeToDelete < Time.time)
                Destroy(PopUp);
    }

    public void InitializePopUp(GameObject popUp, GameObject objectOfEffect, float displayLength, float popUpValue)
    {
        Type type = objectOfEffect.GetComponent<MonoBehaviour>().GetType();

        PopUp = popUp;
        ObjectOfEffect = objectOfEffect;
        AmountToDisplay = (int)popUpValue;
        //DisplayLength = displayLength;

        TimeToDelete = Time.time + displayLength;
        XAxisOffSet = popUp.transform.position.x;
        YAxisOffSet = popUp.transform.position.y;
        FallDirection = UnityEngine.Random.Range(0, 1);

        PopUp_Position = popUp.transform.position;

        _TextMesh = PopUp.GetComponentInChildren<TextMesh>();

        //Vector3 objectOfEffect_Pos = ObjectOfEffect.transform.position;
        ////PopUp_Position = new Vector3(objectOfEffect_Pos.x, objectOfEffect_Pos.y, objectOfEffect_Pos.z - 2f);
        ////PopUp.transform.position = PopUp_Position;

        //// PopUp position is random within a range
        //float _x = UnityEngine.Random.Range(objectOfEffect_Pos.x - 0.1f, objectOfEffect_Pos.x + 0.1f);
        //float _y = UnityEngine.Random.Range(objectOfEffect_Pos.y - 0.1f, objectOfEffect_Pos.y + 0.1f);
        ////float _z = UnityEngine.Random.Range(objectOfEffect_Pos.z - 1.9f, objectOfEffect_Pos.z - 2.1f);
        //float _z = UnityEngine.Random.Range(objectOfEffect_Pos.z - 0.1f, objectOfEffect_Pos.z + 0.1f);
        ////float _z = objectOfEffect_Pos.z -3f;
        //_y = objectOfEffect_Pos.y - 0.3f;

        //PopUp_Position = new Vector3(_x, _y, _z);

        //PopUp.transform.position = PopUp_Position;

        if (type.IsSubclassOf(typeof(Enemy)) || type.IsSubclassOf(typeof(Tower)))
        {
            //Debug.LogError("Type:" + type.ToString());
            DisplayDamageDealt();
        }
        else if (type.IsSubclassOf(typeof(ThraceiumHealingTower)))
        {
            //Debug.LogError("Type:" + type.ToString());
            DisplayHealthGained();
        }
        else
            Debug.LogError("Type is not correctly instantiated.");

        //DeletePopUp();
    }

    private void DisplayDamageDealt()
    {
        _TextMesh.color = Color.red;
        _TextMesh.text = AmountToDisplay.ToString();
    }

    private void DisplayHealthGained()
    {
        string amountToDisplay = "+" + AmountToDisplay.ToString();

        _TextMesh.color = Color.green;
        _TextMesh.text = amountToDisplay;
    }

    // Doesn't work as intended. Using Update() instead.
    //private void DeletePopUp()
    //{
    //    Destroy(PopUp, DisplayLength);
    //}

    #region MessageHandling
    private void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[DamagePopUp] " + message);
    }

    private void LogError(string message)
    {
        Debug.LogError("[DamagePopUp] " + message);
    }

    private void LogWarning(string message)
    {
        if (ShowDebugLogs)
            Debug.LogWarning("[DamagePopUp] " + message);
    }
    #endregion
}