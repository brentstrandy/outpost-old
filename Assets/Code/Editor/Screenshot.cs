using UnityEngine;
using UnityEditor;
using System.IO;

[ExecuteInEditMode]
[CustomEditor(typeof(Camera))]
public class Screenshot : Editor
{
	/*
	public int ResolutionWidth = 3840;
	public int ResolutionHeight = 2160;

	public static string ScreenshotPath(int width, int height) {
		return string.Format("Screenshots/Screenshot {0}x{1} {2}.png",
		                     width, height, 
		                     System.DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
	*/

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Take Screenshot"))
		{
			string path = "Screenshots";
			if (!Directory.Exists(path))
			{
				Debug.Log("Creating screenshot directory.");
				Directory.CreateDirectory(path);
			}
			//
			//Capture();
			string file = string.Format("{0}/Screenshot {1}.png", path, System.DateTime.Now.ToString("yyyyMMddTHHmmss"));
			Application.CaptureScreenshot(file, 4);
			//Debug.Log("Screenshot Taken. Located in 'Screenshots' folder of Outpost (not visible in Unity)");
			Debug.Log(string.Format("Saving screenshot to: {0}", file));
		}
	}

	/*
	// See: http://answers.unity3d.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html
	public void Capture()
	{
		string filename = ScreenshotName(ResolutionWidth, ResolutionHeight);
		Debug.Log("Starting screenshot.");
		screenshotPending = false;
		var camera = GetComponent<Camera>();
		RenderTexture rt = new RenderTexture(ResolutionWidth, ResolutionHeight, 24);
		camera.targetTexture = rt;
		Texture2D screenshot = new Texture2D(ResolutionWidth, ResolutionHeight, TextureFormat.RGB24, false);
		camera.Render();
		RenderTexture.active = rt;
		screenshot.ReadPixels(new Rect(0, 0, ResolutionWidth, ResolutionHeight), 0, 0);
		camera.targetTexture = null;
		RenderTexture.active = null; // JC: added to avoid errors
		DestroyImmediate(rt);
		byte[] bytes = screenshot.EncodeToPNG();
		System.IO.File.WriteAllBytes(filename, bytes);
		Debug.Log(string.Format("Saved screenshot to: {0}", filename));
	}
	*/
}
