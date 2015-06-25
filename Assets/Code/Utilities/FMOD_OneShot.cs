﻿using UnityEngine;
using System.Collections;

public class FMOD_OneShot : MonoBehaviour
{
    public bool ShowDebugLogs = true;

	public string FMODEventName = "";
	public Vector3 Position;

	// Use this for initialization
	void Start ()
	{
		Position = this.transform.position;

        Play_OneShot();
	}

    public void Play_OneShot()
    {
        // TODO: simultaenous calls to same prefab cause audio cut off -- find work around
        FMOD_StudioSystem.instance.PlayOneShot("event:/" + FMODEventName, Position);
    }

    #region Message Handling
    protected virtual void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[FMOD_OneShot] " + message);
    }

    protected virtual void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[FMOD_OneShot] " + message);
    }
    #endregion
}
