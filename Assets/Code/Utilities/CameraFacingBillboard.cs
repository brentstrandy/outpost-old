using UnityEngine;

public class CameraFacingBillboard : MonoBehaviour
{
    private Camera MainCamera;

    private void Start()
    {
        MainCamera = Camera.main;
    }

    private void Update()
    {
        transform.rotation = MainCamera.transform.rotation;
    }
}