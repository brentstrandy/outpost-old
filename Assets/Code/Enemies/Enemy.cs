using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour 
{	
    public bool ShowDebugLogs = true;
	
    public string Name;
    public float Speed = 0.0f;

	// TO DO: This CANNOT be called here. The GameManager Instance throws an error 
	protected GameObject OutpostObject;

	// Use this for initialization
	public virtual void Start () 
	{
		//OutpostObject = GameManager.Instance.OutpostObject;
	}

    // Update is called once per frame
    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {

    }
}
