using UnityEngine;
using System.Collections;

public class ButtonTest : MonoBehaviour 
{
    Canvas can_0;

    public void ButtonPress()
    {
            Debug.Log("Button Press");
    }

    public void ButtonPress(string buttonName)
    {
        Debug.Log(buttonName);
    }

    public void SwitchCamera(string canvasName)
    {
        //Canvas can_all = GameObject.Find("Canvas") as Canvas;
        
        can_0 = GetComponent("Canvas (All)/Canvas0") as Canvas;
        //can_0 = GetComponent<Canvas>();
        //Canvas can_1 = GetComponent(canvasName) as Canvas;

        //if (can_all)
        //{
            

        //    //can_0.enabled = false;
        //    //can_1.enabled = true;
        //}
        //else
        //    Debug.LogError("Cannot find Canvas in scene.");
    }
}