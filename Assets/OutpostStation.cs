using UnityEngine;
using System.Collections;

public class OutpostStation : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Enemy")
			SessionManager.Instance.DestroyObject(other.gameObject);
	}
}
