using UnityEngine;

/// <summary>
/// Attach this script to ANY GameObject and it (and its children) will not be destroyed when a new scene is loaded
/// Owner: Brent Strandy
/// </summary>
public class DontDestroyOnLoad : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
        // Tell unity not to destroy this object when loading a new scene
        DontDestroyOnLoad(this.gameObject);
    }
}