using UnityEngine;
using System.Collections;

public class FMOD_OneShot : MonoBehaviour
{
	public string FMODEventName = "";
	public Vector3 Position;


	// Use this for initialization
	void Start ()
	{
		//if(Position == )
			Position = this.transform.position;

		FMOD_StudioSystem.instance.PlayOneShot("event:/" + FMODEventName, Position);
	}
}
