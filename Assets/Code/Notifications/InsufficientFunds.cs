using UnityEngine;

public class InsufficientFunds : Notification
{
    // Use this for initialization
    public override void Start()
    {
        base.Start();

        //BackgroundImage.color = iTween.ColorTo(this.gameObject, iTween.Hash("a", 0.3,"time", 1, "namedcolorvalue", "_TintColor", "loopType", "pingPong", "easeType", "easeInOutQuad", "includeChildren", true));
    }

    // Update is called once per frame
    public override void Update()
    {
        if (Time.time - StartTime >= TimeToDestroy)
        {
            Destroy(this.gameObject);
        }
    }
}