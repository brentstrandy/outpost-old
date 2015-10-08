using UnityEngine;

public class LaserFire : MonoBehaviour
{
    public Transform Target;
    private LineRenderer Line;

    // Use this for initialization
    private void Start()
    {
        Line = GetComponent<LineRenderer>();
        //Line.SetWidth(0.8f, 0.8f);
        //gameObject.transform.position + gameObject.transform.forward * 50.0f
    }

    // Update is called once per frame
    private void Update()
    {
        Vector3 endpoint;
        if (Target)
        {
            endpoint = Target.position;
            transform.LookAt(endpoint);
        }
        else
        {
            endpoint = transform.position + transform.forward * 50.0f;
        }
        Line.SetPosition(0, transform.position);
        Line.SetPosition(1, endpoint);
    }
}