using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour
{	
	private Camera MainCamera;

	void Start()
	{
		MainCamera = Camera.main;
	}

	void Update()
	{
		transform.rotation = MainCamera.transform.rotation;
	}
}