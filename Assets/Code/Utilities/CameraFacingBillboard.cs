using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour
{	
	private readonly Vector3 Up = new Vector3(0, 0, -1);
	public Vector3 Back = new Vector3(0, 0, 1);

	void Update()
	{
		transform.LookAt(transform.position + Camera.main.transform.rotation * Back,
		                 Camera.main.transform.rotation * Up);
	}
}