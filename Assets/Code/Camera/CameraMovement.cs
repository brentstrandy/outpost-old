using UnityEngine;
using System.Collections;
using Settworks.Hexagons;

/// <summary>
/// Change position and rotate the camera between four views.
/// Owner: John Fitzgerald
/// </summary>
public class CameraMovement : MonoBehaviour 
{
    public bool ShowDebugLogs = true;

    /// <summary>
    /// Different systems to move & rotate the camera.
    /// </summary>
    #region
    private bool FourPointsView = true;
    private bool RightAngleView = false;
    private bool QuadrantView = false;
    private bool HoverMoveView = false;
    #endregion

    private float Smooth; // use for camera lerp

    /// <summary>
    /// [FourPointsView] 4 different points method
    /// </summary>
    #region
    private Vector3 StartPosition, LastPosition;
    private Vector3 NewPosition;
    private float StartRotation, LastRotation;
    private Vector3 NewRotation;
    private float StartCameraFieldOfView, NewCameraFieldOfView;
    private int RotationAmount;
    #endregion

    /// <summary>
    /// [RightAngleView]
    /// </summary>
    #region
    //private GameObject TargetObject;
    //private float RotationAmount;
    //private float OrbitCircumfrance;
    //private float DistanceDegrees;
    //private float DistanceRadians;
    //private float TargetAngle;
    #endregion

    /// <summary>
    /// [HoverMoveView]
    /// </summary>
    #region
    //private float NextEvent;
    #endregion

    /// <summary>
    /// [QuadrantView] Center hexagon in each quadrant.
    /// </summary>
    #region
    //private HexMesh HexagonMap;
    //private HexCoord XYCoordinates;
    //// top right
    //private Vector3 QuadrantCenter_1; // + +
    //// top left
    //private Vector3 QuadrantCenter_2; // - +
    //// bottom left
    //private Vector3 QuadrantCenter_3; // - -
    //// bottom right
    //private Vector3 QuadrantCenter_4; // + -
    //private GameObject Quad1, Quad2, Quad3, Quad4;
    #endregion

    void Start()
    {
        /// <SUMMARY>
        /// [FourPointsView] Set initial camera view
        /// </SUMMARY>
        #region
        Smooth = Time.deltaTime * 1.5f;
        StartPosition = new Vector3(10.8f, 9.4f, -23f);
        StartRotation = 0;
        NewRotation = NewPosition = new Vector3(0, 0, 0);
        StartCameraFieldOfView = Camera.main.fieldOfView = 54;

        RotationAmount = 0;

        transform.position = StartPosition;
        transform.eulerAngles = new Vector3(0, 0, StartRotation);

        LastPosition = StartPosition;
        LastRotation = StartRotation;
        #endregion

        /// <summary>
        /// [RightAngleView]
        /// </summary>
        #region
        //TargetObject = GameObject.Find("Mining Facility");
        //RotationAmount = 1.5f;
        //TargetAngle = 0;        
        #endregion

        /// <summary>
        /// [HoverMoveView]
        /// </summary>
        #region
        //NextEvent = Time.time;
        #endregion

        /// <SUMMARY>
        /// [QuadrantView] When Josh's coordinate code is implemented, try this.
        /// <///SUMMARY>
        #region
        //HexagonMap = GetComponent<HexMesh>();
        //XYCoordinates = new HexCoord();
        //XYCoordinates = HexCoord.AtPosition(new Vector2(HexagonMap.GridWidth * 0.75f, HexagonMap.GridHeight * 0.75f));
        //Debug.LogError("[START] X: " + XYCoordinates.q + " Y: " + XYCoordinates.r);
        
        //QuadrantCenter_1 = new Vector3(XYCoordinates.q, XYCoordinates.r, 0);
        //QuadrantCenter_2 = new Vector3(-XYCoordinates.q, XYCoordinates.r, 0);
        //QuadrantCenter_3 = new Vector3(-XYCoordinates.q, -XYCoordinates.r, 0);
        //QuadrantCenter_4 = new Vector3(XYCoordinates.q, -XYCoordinates.r, 0);

        //Quad1 = GameObject.Find("HexOutline630");
        //Quad2 = GameObject.Find("HexOutline620");
        //Quad3 = GameObject.Find("HexOutline210");
        //Quad4 = GameObject.Find("HexOutline220");


        //Vector3 EyeingItMiddle = new Vector3(16.9f, 7.8f, 0);
        //QuadrantCenter_1 = new Vector3(EyeingItMiddle.x, EyeingItMiddle.y, 0);
        //QuadrantCenter_2 = new Vector3(-EyeingItMiddle.x, EyeingItMiddle.y, 0);
        //QuadrantCenter_3 = new Vector3(-EyeingItMiddle.x, -EyeingItMiddle.y, 0);
        //QuadrantCenter_4 = new Vector3(EyeingItMiddle.x, -EyeingItMiddle.y, 0);
        #endregion
    }

    void Update()
    {
        #region FOUR POINTS VIEW
        if (FourPointsView)
        {
            if (Input.GetKeyDown("left")) // +90deg (left isn't negative because the XY-plane is our ground plane instead of XZ-plane)
                RotationAmount += 90; 
            if (Input.GetKeyDown("right")) // -90deg (right isn't positive because the XY-plane is our ground plane instead of XZ-plane)
                RotationAmount -= 90;

            if(RotationAmount != 0)
                Rotate_FourPointsView();
        }
        #endregion

        #region RIGHT ANGLE VIEW
        //if (RightAngleView)
        //{
        //        if (Input.GetKeyDown("left"))
        //        {
        //            Log("left arrow key is held down");
        //            TargetAngle -= 90.0f;

                    //transform.Rotate(0, 0, -90);
                    
                    //float tiltAroundX = Input.GetAxis("Horizontal") * 330f;
                    //float tiltAroundZ = Input.GetAxis("Vertical") * 330f;

                    //Quaternion target = Quaternion.Euler(tiltAroundX, transform.position.y, tiltAroundZ);
                    //transform.rotation = Quaternion.Slerp(transform.rotation, target, Smooth * Time.deltaTime);

                    //transform.position = new Vector3(QuadrantCenter_2.x, QuadrantCenter_2.y, transform.position.z);
                    //transform.LookAt(Quad2.transform);
                //}
                //if (Input.GetKeyDown("right"))
                //{
                //    Log("right arrow key is held down");
                //    TargetAngle += 90.0f;

                    //transform.position = new Vector3(QuadrantCenter_4.x, QuadrantCenter_4.y, transform.position.z);
                    //transform.LookAt(Quad4.transform);
                //}

                //if (TargetAngle != 0)
                //{
        //    StartCoroutine("Rotate_RightAngleView");
        //    //StopCoroutine("Rotate_RightAngleView");
                //}
        //}
        #endregion

        #region HOVER MOVE VIEW
        //if (HoverMoveView)
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
        #endregion

        #region QUADRANT CAMERA VIEW
        //XYCoordinates = HexCoord.AtPosition(new Vector2(HexagonMap.GridWidth * 0.75f, HexagonMap.GridHeight * 0.75f));

        //if (QuadrantView)
        //{
        //    if (Input.GetKey("up"))
        //    {
        //        Debug.LogError("[UP] X: " + XYCoordinates.q + "Y: " + XYCoordinates.r);

        //        Vector3 EyeingItMiddle = new Vector3(16.9f, 7.8f, 0);
        //        QuadrantCenter_1 = new Vector3(EyeingItMiddle.x, EyeingItMiddle.y, 0);
        //        Debug.LogError("[Quad1] x:" + QuadrantCenter_1.x + " y: " + QuadrantCenter_1.y);
        //        Log("up arrow key is held down");

        //        transform.position = Vector3.Lerp(transform.position, new Vector3(QuadrantCenter_1.x, QuadrantCenter_1.y, transform.position.z), Smooth * Time.deltaTime);

        //        float tiltAroundZ = Input.GetAxis("Horizontal") * 330f;
        //        float tiltAroundX = Input.GetAxis("Vertical") * 330f;

        //        //Quaternion target = Quaternion.Euler(tiltAroundX, 0, tiltAroundZ);
        //        //transform.rotation = Quaternion.Slerp(transform.rotation, target, Smooth * Time.deltaTime);

        //        //transform.LookAt(Quad1.transform);
        //    }
        //    if (Input.GetKey("left"))
        //    {
        //        Log("left arrow key is held down");
        //        transform.position = new Vector3(QuadrantCenter_2.x, QuadrantCenter_2.y, transform.position.z);
        //        //transform.LookAt(Quad2.transform);
        //    }
        //    if (Input.GetKey("down"))
        //    {
        //        Log("down arrow key is held down");
        //        transform.position = new Vector3(QuadrantCenter_3.x, QuadrantCenter_3.y, transform.position.z);
        //        //transform.LookAt(Quad3.transform);
        //    }
        //    if (Input.GetKey("right"))
        //    {
        //        Log("right arrow key is held down");
        //        transform.position = new Vector3(QuadrantCenter_4.x, QuadrantCenter_4.y, transform.position.z);
        //        //transform.LookAt(Quad4.transform);
        //    }

        //    if (Input.GetKey("up"))
        //    {
        //        Log("up arrow key is held down");
        //        Quad1 = GameObject.Find("HexOutline630");
        //        transform.position = new Vector3(Quad1.transform.position.x, Quad1.transform.position.y, transform.position.z);
        //        //transform.LookAt(Quad1.transform);
        //    }
        //    if (Input.GetKey("left"))
        //    {
        //        Log("left arrow key is held down");
        //        Quad2 = GameObject.Find("HexOutline620");
        //        transform.position = new Vector3(Quad2.transform.position.x, Quad2.transform.position.y, transform.position.z);
        //        //transform.LookAt(Quad2.transform);
        //    }
        //    if (Input.GetKey("down"))
        //    {
        //        Log("down arrow key is held down");
        //        Quad3 = GameObject.Find("HexOutline210");
        //        transform.position = new Vector3(Quad3.transform.position.x, Quad3.transform.position.y, transform.position.z);
        //        //transform.LookAt(Quad3.transform);
        //    }
        //    if (Input.GetKey("right"))
        //    {
        //        Log("right arrow key is held down");
        //        Quad4 = GameObject.Find("HexOutline220");
        //        transform.position = new Vector3(Quad4.transform.position.x, Quad4.transform.position.y, transform.position.z);
        //        //transform.LookAt(Quad4.transform);
        //    }
        //}
        #endregion
    }

    /// <summary>
    /// Rotate left or right (based on FixedUpdate user input)
    /// </summary>
    private void Rotate_FourPointsView()
    {
        if (LastRotation == 0 && RotationAmount == -90)
            NewRotation.z = 270;
        else if (LastRotation == 270 && RotationAmount == 90)
            NewRotation.z = 0;
        else
            NewRotation.z = Mathf.Abs(RotationAmount + (int)LastRotation);

        // hold old rotation for LERPing
        //Quaternion oldRotation = transform.rotation;

        switch ((int)NewRotation.z)
        {
            case 0:
                NewCameraFieldOfView = StartCameraFieldOfView;
                NewPosition = StartPosition;
                break;
            case 90:
                NewCameraFieldOfView = 70f;
                NewPosition = new Vector3(-15.1f, 24.5f, StartPosition.z);
                break;
            case 180:
                NewCameraFieldOfView = StartCameraFieldOfView;
                NewPosition = new Vector3(-StartPosition.x, -5.23f, StartPosition.z);
                break;
            case 270:
                NewCameraFieldOfView = 70f;
                NewPosition = new Vector3(15.1f, -20.4f, StartPosition.z);
                break;
            default:
                NewCameraFieldOfView = StartCameraFieldOfView;
                NewPosition = StartPosition;
                NewRotation.z = StartRotation;
                break;
        }

        Camera.main.fieldOfView = NewCameraFieldOfView;
        transform.position = NewPosition;
        transform.eulerAngles = NewRotation;
        
        #region ATTEMPT AT LERPING CAMERA
        //Quaternion tempQuat = Quaternion.Euler(0, 0, NewRotation.z);
        
        //transform.position = Vector3.Lerp(transform.position, newRotation, Smooth);
        //transform.rotation = Quaternion.Lerp(transform.rotation, tempQuat, Smooth);
        //transform.localScale = Vector3.Lerp(transform.localScale, , Smooth);

        //Quaternion newRotation = Quaternion.Euler(0, 0, NewRotation.z);
        //newRotation = Quaternion.Euler(0, 0, NewRotation.z);
        // rotate camera around Z-axis (use LERP for linear interpolation)
        //transform.rotation = Quaternion.Lerp(oldRotation, newRotation, Smooth);
        #endregion

        if (ShowDebugLogs)
            Log("[Case]: " + NewRotation.z);

        LastRotation = NewRotation.z;
        RotationAmount = 0;
    }

    //IEnumerator Rotate_RightAngleView()
    //{
        //OrbitCircumfrance = 2F * 1f * Mathf.PI;
        //DistanceDegrees = (1f / OrbitCircumfrance) * 360;
        //DistanceRadians = (1f / OrbitCircumfrance) * 2 * Mathf.PI;

        //if (TargetAngle > 0)
        //{
        //    transform.Rotate(0, 0, TargetAngle);
        //    //transform.RotateAround(TargetObject.transform.position, Vector3.down, -RotationAmount);
            
        //    TargetAngle -= 90f;
        //}
        //else if (TargetAngle < 0)
        //{
        //    transform.Rotate(0, 0, TargetAngle);
        //    //transform.RotateAround(TargetObject.transform.position, Vector3.up, RotationAmount); 
            
        //    TargetAngle += 90;
        //}

    //    yield return new WaitForSeconds(3);
    //}

    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[CameraMovement] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[CameraMovement] " + message);
    }
    #endregion
}