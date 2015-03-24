using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour 
{
    // different systems to move camera
    private bool QuadrantView = true;
    private bool HoverMove = true;

    private float Smooth = 1.5f;
    private HexMesh HexagonMap;
    // FITZGERALD: change to coroutine
    private float NextEvent;

    /// <summary>
    /// Center hexagon in each quadrant.
    /// </summary>
    // top right
    private Vector3 Quadrant1Center;
    // top left
    private Vector3 Quadrant2Center;
    // bottom left
    private Vector3 Quadrant3Center;
    // bottom right
    private Vector3 Quadrant4Center;


    void Awake()
    {
        NextEvent = Time.time;

        HexagonMap = GetComponent<HexMesh>();
        //Quadrant1Center = HexagonMap
        // determine center hexagon in each quadrant

        // not needed - just attach this script to Camera GO, and transform will refer to it
        //CameraSwitchQuick = GetComponent<Camera>();
    }

    void Update()
    {
        #region QUADRANT CAMERA MOVE
        if (QuadrantView)
        {

        }
        #endregion

        //#region HOVER-MOUSE CAMERA MOVE
        //if (HoverMove)
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit hit;

        //    // move camera to mouse-over hexagon's position
        //    if (Physics.Raycast(ray, out hit))
        //    {
        //        //StartCoroutine(LerpToPosition(hit));
                
        //        if (Time.time >= NextEvent)
        //        {
        //            NextEvent += 3f;
        //            Debug.Log("[Camera] x: " + hit.transform.position.x + " y: " + transform.position.y + " z: " + hit.transform.position.z);

        //            Vector3 oldPos = transform.position;
        //            Vector3 newPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);

        //            transform.position = Vector3.Lerp(oldPos, newPos, Smooth * Time.time);
        //            //transform.LookAt(newPos);

        //            //if (hit.collider == GetComponent<MeshCollider>())
        //            //{
        //            //    Debug.LogError("CameraSwitch");
        //            //    transform.position = hit.transform.position;//hit.triangleIndex;
        //            //}
        //        }
        //    }
        //}
        //#endregion
    }

    IEnumerator LerpToPosition(RaycastHit hit)
    {
        yield return 0;
    }
}