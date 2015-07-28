using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
[CustomEditor(typeof(Camera))]
public class Screenshot : Editor
{
	// Use this for initialization
	void Start ()
	{
	
	}

	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Take Screenshot"))
		{
			Debug.Log("Screenshot Taken. Located in 'Screenshots' folder of Outpost (not visible in Unity)");
			Application.CaptureScreenshot("Screenshots/screenshot.png", 4);
		}
	}
}
