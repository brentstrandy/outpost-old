using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DataManager<T>
{
	public bool ShowDebugLogs = false;

	public bool FinishedLoadingData { get; private set; }

	// Events fired when data finishes downloading
	public delegate void DataManagerAction();
	public event DataManagerAction OnDataLoadSuccess;
	public event DataManagerAction OnDataLoadFailure;

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

	/// <summary>
	/// Method used to load XML from a local location
	/// </summary>
	/// <param name="filename">Filename.</param>
	public void LoadDataFromLocal(string filename)
	{
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
	public IEnumerator LoadDataFromServer(string filename, WWWForm form = null)
	{
		WWW www;
		string myXML;
		if(form != null)
			www = new WWW(filename, form);
		else
			www = new WWW(filename);

		Log ("About to download Data: " + filename);
		while(!www.isDone)
		{
			yield return 0;
		}

		// Check for an error in processing before continuing
		if(string.IsNullOrEmpty(www.error) && !string.IsNullOrEmpty(www.text))
		{
			myXML = www.text;

			// Deserialize XML and add each enemy spawn to the lists
			foreach (T data in XMLParser<T>.XMLDeserializer_Server(myXML))
			{
				DataList.Add(data);
			}

			FinishedLoadingData = true;

			if(OnDataLoadSuccess != null)
				OnDataLoadSuccess();

			Log ("Loaded " + typeof(T) + " (Server)");
		}
		else
		{
			LogError("Failed downloading " + typeof(T) + " from server. Error: " + www.error.ToString());
			if(OnDataLoadFailure != null)
				OnDataLoadFailure();
		}
	}

	/// <summary>
	/// Total number of data items
	/// </summary>
	/// <returns>Total Count</returns>
	public int Count()
	{
		return DataList.Count;
	}

	/// <summary>
	/// Clears all currently loaded data items
	/// </summary>
	public void ClearData()
	{
		FinishedLoadingData = false;
		DataList.Clear();
	}

	public void SortDataList()
	{
		DataList.Sort();
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[DataManager] " + message);
	}

	protected void LogError(string message)
	{
		Debug.LogError("[DataManager] " + message);
	}
	#endregion

}

