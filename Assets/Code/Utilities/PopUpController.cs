using System;
using UnityEngine;

/// <summary>
/// Displays heal and damage numbers above GameObjects.
/// Owner: John Fitzgerald
/// </summary>
public class PopUpController : MonoBehaviour
{
    public bool ShowDebugLogs = true;

    private string PopUpHolderName = "PopUpHolder";
    private string PopUpHolderLocation;

    private GameObject ObjectOfEffect;
    private GameObject PopUp;
    private GameObject PopUpHolder;

    public int AmountToDisplay;
    public float DeleteTime;

    public float FadeSpeed = 4f;
    public float FloatSpeed = 2f;

    //public float XAxisOffSet;
    //public float YAxisOffSet;
    //public float ZAxisOffSet;
    //public float FallDirection;

    public Vector3 PopUp_Position;
    public TextMesh _TextMesh;

    // Update is called once per frame
    private void Update()
    {
        Color currentColor = GetComponent<TextMesh>().color;
        Vector3 position = transform.position;

        // fade the color of numbers over time
        currentColor.a = Mathf.Lerp(currentColor.a, 0, Time.deltaTime * FadeSpeed);
        GetComponent<TextMesh>().color = currentColor;

        // the numbers appear drop
        position.z = Mathf.Lerp(position.z, position.z + 0.8f, Time.deltaTime * FloatSpeed);
        PopUp.transform.position = position;

        //if (currentColor.a < 0.5f)
        //    PopUp.SetActive(false);

        //ZAxisOffSet += 0.08f;
        // Numbers will fall "down" and to the side
        //PopUp.transform.position = new Vector3(XAxisOffSet, YAxisOffSet, ZAxisOffSet);

        if (DeleteTime <= Time.time || ObjectOfEffect == null)
            Destroy(PopUp);
    }

    public void InitializePopUp(GameObject popUp, GameObject objectOfEffect, float displayLength, float popUpValue, Type type)
    {
        PopUpHolderLocation = "Utilities/" + PopUpHolderName;

        PopUp = popUp;
        ObjectOfEffect = objectOfEffect;

        AmountToDisplay = (int)popUpValue;
        DeleteTime = Time.time + displayLength;

        //XAxisOffSet = PopUp.transform.position.x;
        //YAxisOffSet = PopUp.transform.position.y;
        //ZAxisOffSet = PopUp.transform.position.z;
        //FallDirection = UnityEngine.Random.Range(0, 1);

        PopUpHolder = GameObject.FindGameObjectWithTag("PopUpHolder");

        // Set parent to PopUpHolder -- create PopUpHolder in scene if not found
        if (!PopUpHolder)
            PopUpHolder = ScriptableObject.Instantiate(Resources.Load(PopUpHolderLocation)) as GameObject;
        PopUp.transform.parent = PopUpHolder.transform;

        // Set initial position to enemy
        PopUp_Position = PopUp.transform.position;
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

    #endregion MessageHandling
}