using UnityEngine;
using System.Collections;

public class DimLightOverTime : MonoBehaviour
{    
	public float Duration = 5;

	private Light LightComponent;
	private float StartIntensity;
	private float StartTime;

	private void Awake()
	{
		LightComponent = this.GetComponentInChildren<Light>();

		StartIntensity = LightComponent.intensity;
		StartTime = Time.time;
	}

	private void Update()
	{
		if(LightComponent)
		{
			if(LightComponent.intensity > 0.05f)
				LightComponent.intensity = Mathf.Lerp(StartIntensity, 0, (Time.time - StartTime) / Duration);
			else
				Destroy(this);
		}
	}
}
