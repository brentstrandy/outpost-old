using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class DataManager<T>
{
	public bool ShowDebugLogs = false;
	
	public bool FinishedLoadingData { get; private set; }
	 
	/// <summary>
	/// Gets the Data list.
	/// </summary>
	/// <value>The data list.</value>
	public List<T> DataList { get; private set; }


	public DataManager()
	{
		FinishedLoadingData = false;
		
		// Instantiate lists to save the data
		DataList = new List<T>();
	}
	
	public void LoadDataFromLocal(string filename)
	{
		//location of level specific XML spawn data
		string levelDataXMLPath = Application.streamingAssetsPath + "/" + filename;

		if (File.Exists(levelDataXMLPath))
		{
			foreach (T data in XMLParser<T>.XMLDeserializer_Local(levelDataXMLPath))
			{
				DataList.Add(data);
			}

			FinishedLoadingData = true;
			Log ("Loaded " + typeof(T) + " (Local)");
		}
		else
			LogError("Cannot find " + filename + " XML data file");
	}
	
	/// <summary>
	/// Coroutine method used to load XML data from a server location
	/// </summary>
	public IEnumerator LoadDataFromServer(string filename)
	{
		WWW www = new WWW("http://www.diademstudios.com/outpostdata/" + filename);
		string myXML;
		
		while(!www.isDone)
		{
			yield return 0;
		}

		myXML = www.text;
		
		// Deserialize XML and add each enemy spawn to the lists
		foreach (T data in XMLParser<T>.XMLDeserializer_Server(myXML))
		{
			DataList.Add(data);
		}

		FinishedLoadingData = true;
		Log ("Loaded " + typeof(T) + " (Server)");
	}
	
	/// <summary>
	/// Total number of Levels
	/// </summary>
	/// <returns>Total Count</returns>
	public int Count()
	{
		return DataList.Count;
	}

	/// <summary>
	/// Clears all currently loaded LevelData
	/// </summary>
	public void ClearData()
	{
		DataList.Clear();
	}
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[DataManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[DataManager] " + message);
	}
	#endregion

}

