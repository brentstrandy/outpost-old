using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float TimeToDestroy = 2.0f;
    private float StartTime;

    // Use this for initialization
    private void Start()
    {
        StartTime = Time.time;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Time.time - StartTime > TimeToDestroy)
            Destroy(this.gameObject);
    }
}