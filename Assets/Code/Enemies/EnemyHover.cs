using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyHover : MonoBehaviour 
{
    // 1) Script to give rigidbody2d ability to sway/tilt like a hovercraft
    bool ShowDebugLogs = true;
    bool IsPhysicsSetup;

    public float SpeedUpdate;
    public float ForwardPower;
    public float SteerPower;
    public float LandingPower;
    public float JumpingPower;
    public float HoverHeight;
    public float Stability;

    private GameObject HoverBody;
    private string HoverBodyName;
    private Rigidbody HoverBodyRigidbody;

    private BoxCollider HoverBoxCollider;
    private Vector3 HoverBoxDimension;
    private Vector3[] HoverPoints = new Vector3[5];
    private Vector3[] HitNormal = new Vector3[5];
    private Transform[] PlatformCorners = new Transform[5];

    private Quaternion Rotation;
    private float Increment;
    private Vector3[] LastNormals = new Vector3[5];
    private float ZBounce;      // was Y
    private Vector3 LastPosition;
    private float Distance;
    private Vector3 Average;

    // FITZGERALD: Change all "up"'s to reflect this "Up"
    private readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);

	void Awake () 
    {
        HoverBody = this.gameObject;
        HoverBodyName = this.gameObject.name;
        HoverBodyRigidbody = this.GetComponent<Rigidbody>();

        // find type of enemy that the script is attached to
        if (HoverBodyName.Contains("Light"))
        {
            ForwardPower = 750f;
            SteerPower = 350f;
            Stability = 1f;
            Debug.Log("LIGHT");
        }
        else if (HoverBodyName.Contains("Heavy"))
        {
            ForwardPower = 750f;
            SteerPower = 350f;
            Stability = 1f;
            Debug.Log("HEAVY");
	    }
        else
        {
            LogError("Script is not attached to a valid GameObject.");
        }

        InitializePhysics();
        // 
     }
	
	void Update () 
    {
	    //this.GetComponent<Rigidbody2D>().AddForce
        UpdateSpeed();

	}

    void FixedUpdate()
    {
        if (IsPhysicsSetup)
        {
            RaycastHit hit;

            for (int i = 0; i < PlatformCorners.Length; i++)
            {
                if (Physics.Raycast(PlatformCorners[i].position, -PlatformCorners[i].up, out hit, HoverHeight + 100))
                {
                    HitNormal[i] = HoverBody.transform.InverseTransformDirection(hit.normal);
                    if (LastNormals[i] != HitNormal[i])
                    {
                        Increment = 0;
                        LastNormals[i] = HitNormal[i];
                    }

                    Distance = hit.distance;
                    if (hit.distance < HoverHeight)
                        GetComponent<ConstantForce>().relativeForce = (-Average + transform.up) * GetComponent<Rigidbody>().mass * JumpingPower * GetComponent<Rigidbody>().drag * Mathf.Min(HoverHeight, HoverHeight / Distance);
                    else
                        GetComponent<ConstantForce>().relativeForce = -(transform.up) * GetComponent<Rigidbody>().mass * LandingPower * GetComponent<Rigidbody>().drag / Mathf.Min(HoverHeight / Distance);
                }
                else
                    GetComponent<ConstantForce>().relativeForce = -(transform.up) * GetComponent<Rigidbody>().mass * LandingPower * GetComponent<Rigidbody>().drag * 6 * (1 - Input.GetAxis("Vertical"));
            }
        }
        
        foreach (var normal in HitNormal)
            Average += normal;
        Average /= -2f;

        if (Increment != 1f)
            Increment += 0.03f;

        // reorient with the ground
        Rotation = Quaternion.Slerp(HoverBody.transform.localRotation, Quaternion.Euler(Average * Mathf.Rad2Deg), Increment);
        Quaternion tempRotation = Rotation;

        tempRotation.y = transform.up.y * Mathf.Deg2Rad;
        HoverBody.transform.localRotation = tempRotation;
        //HoverBody.transform.localRotation. = transform.up.y * Mathf.Deg2Rad;


        float ForwardForce = Input.GetAxis("Vertical") * ForwardPower;
        GetComponent<Rigidbody>().AddForce(transform.forward * ForwardForce);

        float SteerForce = Input.GetAxis("Horizontal") * SteerPower;
        GetComponent<Rigidbody>().AddTorque(transform.up * SteerForce);

    }

    /// <summary>
    /// Displays hover corners (for debug)
    /// </summary>
    private void OnDrawGizmos()
    {
        if (ShowDebugLogs)
        {
            for (int i = 0; i < HoverPoints.Length; i++)
            {
                if (PlatformCorners[i] != null)
                    Gizmos.DrawWireSphere(PlatformCorners[i].position, 0.10f);
            }
        }
    }

    private void UpdateSpeed()
    {
        //if (LastPosition != transform.position)
        //{
        //    float distance = Vector3.Distance(transform.position, LastPosition);
        //    //SpeedUpdate = (distance / 1000) / (Time.deltaTime / 3600); // km/h
        //    SpeedUpdate = (distance / 3.28f) / (Time.deltaTime / 5280);
        //}
    }

    private void InitializePhysics()
    {
        HoverBoxCollider = gameObject.AddComponent<BoxCollider>();

        // Store positions for hover center of mass and corners
        HoverBoxDimension = new Vector3(HoverBoxCollider.size.x * HoverBody.transform.localScale.x, HoverBoxCollider.size.y * HoverBody.transform.localScale.y, HoverBoxCollider.size.z * HoverBody.transform.localScale.z * Stability);
        HoverPoints[0] = new Vector3(transform.position.x - HoverBoxDimension.x/2, transform.position.y - HoverBoxDimension.y/2, transform.position.z + HoverBoxDimension.z * 1.5f);
        HoverPoints[1] = new Vector3(HoverBoxDimension.x/2 + transform.position.x, transform.position.y - HoverBoxDimension.y/2, transform.position.z + HoverBoxDimension.z * 1.5f);
        HoverPoints[2] = new Vector3(HoverBoxDimension.x/2 + transform.position.x, transform.position.y - HoverBoxDimension.y/2, transform.position.z - HoverBoxDimension.z * 1.5f);
        HoverPoints[3] = new Vector3(transform.position.x - HoverBoxDimension.x/2, transform.position.y - HoverBoxDimension.y/2, transform.position.z - HoverBoxDimension.z * 1.5f);
        HoverPoints[4] = transform.position;
        Destroy(HoverBoxCollider);

        // Create five Spheres 
        for (int i = 0; i < HoverPoints.Length; i++)
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            platform.name = "Platform" + "[" + i + "]";
            platform.transform.parent = HoverBody.transform;
            platform.transform.localPosition = transform.InverseTransformPoint(HoverPoints[i]);
            PlatformCorners[i] = platform.transform;
            Destroy(platform.GetComponent<MeshRenderer>());
            Destroy(platform.GetComponent<Collider>());
        }
        //HoverPoints = null;
        IsPhysicsSetup = true;
    }

    /// <SUMMARY>
    /// http://answers.unity3d.com/questions/456400/how-to-face-hovercraft-physics.html
    /// </SUMMARY>
    //public List<Transform> HoverPoints = new List<Transform>();
    //public float HoverHeight = -0.5f;//7;
    //public float HoverForceFront = 200;
    //public float HoverForceBack = 400;
    //void FixedUpdate()
    //{
    //    bool isGrounded;

    //    //Lift
    //    for (int i = 0; i < 4; i++)
    //    {
    //        RaycastHit Hit;

    //        if (i > 1)
    //        {
    //            if (Physics.Raycast(HoverPoints[i].position, HoverPoints[i].TransformDirection(Vector3.down), out Hit, HoverHeight))
    //            {
    //                GameObjectRigidbody2D.AddForceAtPosition((Vector3.up * HoverForceBack * Time.deltaTime) *
    //                                                          Mathf.Abs(1 - (Vector3.Distance(Hit.point, HoverPoints[i].position) / 
    //                                                          HoverHeight)), HoverPoints[i].position);
    //            }
    //            if (Hit.point != Vector3.zero)
    //                Debug.DrawLine(HoverPoints[i].position, Hit.point, Color.blue);
    //        }
    //        else
    //        {
    //            if (Physics.Raycast(HoverPoints[i].position, HoverPoints[i].TransformDirection(Vector3.down), out Hit, HoverHeight))
    //            {
    //                GameObjectRigidbody2D.AddForceAtPosition((Vector3.up * HoverForceFront * Time.deltaTime) *
    //                                                          Mathf.Abs(1 - (Vector3.Distance(Hit.point, HoverPoints[i].position) / 
    //                                                          HoverHeight)), HoverPoints[i].position);
    //            }
    //            if (Hit.point != Vector3.zero)
    //                Debug.DrawLine(HoverPoints[i].position, Hit.point, Color.red);
    //        }

    //        if (Hit.point != Vector3.zero)
    //            isGrounded = true;
    //        else
    //            isGrounded = false;
    //    }
    //}

    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[EnemySway - " + HoverBodyName + "] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[EnemySway - " + HoverBodyName + "] " + message);
    }
    #endregion
}
