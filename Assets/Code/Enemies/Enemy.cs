using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour 
{	
    public bool ShowDebugLogs = true;
	
    public string Name;
    public float Speed = 0.0f;
	
	protected GameObject OutpostObject;

	public virtual void Awake()
	{
		OutpostObject = GameManager.Instance.MiningFacility;

		EnemyManager.Instance.AddActiveEnemy(this);
	}

	// Use this for initialization
	public virtual void Start () 
	{
		//OutpostObject = GameManager.Instance.OutpostObject;
	}

    // Update is called once per frame
    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {

    }

	public void OnDestroy()
	{
		// Tell the enemy manager this enemy is being destroyed
		EnemyManager.Instance.RemoveActiveEnemy(this);
	}
}
