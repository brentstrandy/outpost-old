using Settworks.Hexagons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool ShowDebugLogs = false;
    protected readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);

    // Enemy Attributes/Details/Data
    public EnemyData EnemyAttributes;

    protected float Health;

    protected float TimeLastShotFired;
    protected Vector3 CurAcceleration;
    protected Vector3 CurVelocity;
	protected float CurHeight;

    // Pathfinding
    protected PathFindingType PathFinding;

    protected GameObject TargetedObjectToFollow;
    protected GameObject TargetedObjectToAttack;
    protected TargetType TargetType
    {
        get
        {
            if (TargetedObjectToAttack == null)
            {
                return TargetType.None;
            }
            if (TargetedObjectToAttack.GetComponent<MiningFacility>() != null)
            {
                return TargetType.MiningFacility;
            }
            if (TargetedObjectToAttack.GetComponent<Tower>() != null)
            {
                return TargetType.Tower;
            }
            return TargetType.None;
        }
    }

    protected Quadrant CurrentQuadrant;

    protected Vector2 DestinationHex;

    public Vector3 Destination
    {
        get
        {
            var destination = (Vector3)DestinationHex;
            if (GameManager.Instance.TerrainMesh != null)
            {
                destination.z = GameManager.Instance.TerrainMesh.Map.Surface.Intersect(DestinationHex) - EnemyAttributes.HoverDistance; // Note: Up is negative Z
            }
            return destination;
        }
    }

    // Components
    public HealthBarController HealthBar;

    public GameObject TurretPivot;
    public GameObject[] FireEffectEmitters;
    protected PhotonView ObjPhotonView;
    protected Pathfinder ObjPathfinder = null;
    protected HexLocation ObjHexLocation = null;

    public int NetworkViewID { get; private set; }
    public Analytics_Asset AnalyticsAsset;

    // Effects
    public GameObject FiringEffect;

    public GameObject ExplodingEffect;

    // Status Effects
    public bool IsStunned { get; private set; }

    public float StunEndTime { get; private set; }

    #region INITIALIZATION

    public virtual void Awake()
    {
    }

    // Use this for initialization
    public virtual void Start()
    {
        // Add the enemy to the EnemyManager object to track the Enemy
        GameManager.Instance.EnemyManager.AddActiveEnemy(this);

        // Allow the first bullet to be fired immediatly after the enemy is instantiated
        TimeLastShotFired = 0;
    }

    /// <summary>
    /// Instantiates the Enemy based on EnemyData. The Enemy will create Components where needed
    /// </summary>
    /// <param name="enemyData">EnemyData to be used when instantiating Enemy</param>
    public void SetEnemyData(EnemyData enemyData)
    {
        EnemyAttributes = enemyData;

        // Save reference to PhotonView
        ObjPhotonView = PhotonView.Get(this);
        NetworkViewID = ObjPhotonView.viewID;

        // PhotonView does not instantiate the ObservedComponents list - you must instantiate this list before attempting
        // to add any items into it.
        ObjPhotonView.ObservedComponents = new List<Component>();
        ObjPhotonView.synchronization = ViewSynchronization.Off;

        PathFinding = EnemyAttributes.PathFinding;

        IsStunned = false;

        // Initialize the proper pathfinding
        if (PathFinding == PathFindingType.ShortestPath)
        {
            if (gameObject.GetComponent<Pathfinder>() == null)
                ObjPathfinder = gameObject.AddComponent<Pathfinder>();
            else
                ObjPathfinder = gameObject.GetComponent<Pathfinder>();

            // By default, avoid towers
            //ObjPathfinder.AvoidTowers = true;
            ObjPathfinder.OnPathfindingFailure = OnPathfindingFailure;

            if (gameObject.GetComponent<HexLocation>() == null)
                ObjHexLocation = gameObject.AddComponent<HexLocation>();
            else
                ObjHexLocation = gameObject.GetComponent<HexLocation>();

            ObjHexLocation.layout = HexCoord.Layout.Horizontal;
            ObjHexLocation.autoSnap = false;
            ObjHexLocation.gridScale = 1;
            ObjHexLocation.ApplyPosition();

            DestinationHex = ObjPathfinder.Next().Position();
        }
        else
        {
            ObjPhotonView.synchronization = ViewSynchronization.ReliableDeltaCompressed;
            ObjPhotonView.ObservedComponents.Add(this.transform);
            ObjPhotonView.onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;
        }

        // Start a coroutine to attack the mining facility and towers (if applicable)
        if (EnemyAttributes.AttackMiningFacility || EnemyAttributes.AttackTowers)
            StartCoroutine("Fire");

        Health = EnemyAttributes.MaxHealth;

        // Only initialize the health bar if it is used for this enemy
        if (HealthBar)
            HealthBar.InitializeBars(EnemyAttributes.MaxHealth);

        // If the enemy has range attack, add a sphere collider to detect range
        if (EnemyAttributes.Range > 0)
        {
            GameObject go = new GameObject("Awareness", typeof(SphereCollider));
            go.GetComponent<SphereCollider>().isTrigger = true;
            go.GetComponent<SphereCollider>().radius = EnemyAttributes.Range * 2;
            go.transform.parent = this.transform;
            go.transform.localPosition = Vector3.zero;
        }

        // Set the enemy's highlight color
        //gameObject.GetComponentInChildren<Renderer>().material.color = EnemyAttributes.HighlightColor;

		// Set the player's color on any material titled "PlayerColorMaterial"
		foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
		{
			foreach (Material material in renderer.materials)
			{
				if (material.name.Contains("PlayerColorMaterial"))
				{
					material.SetColor("_Color", EnemyAttributes.HighlightColor);
					//material.SetColor("_EmissionColor", EnemyAttributes.HighlightColor);
				}
			}
		}

        // Store a reference to the AnalyticsManager's information on this Enemy
        if(GameManager.Instance.GameRunning)
            AnalyticsAsset = AnalyticsManager.Instance.Assets.FindAsset("Enemy", EnemyAttributes.DisplayName, NetworkViewID);
    }

    #endregion INITIALIZATION

    public virtual void OnPathfindingFailure()
    {
        // TODO: Target the nearest tower?
    }

    public virtual void Update()
    {
        // Un-stun enemy when StunEndTime has passed.
        if (StunEndTime > 0 && Time.time > StunEndTime)
        {
            IsStunned = false;
            StunEndTime = 0;
        }

        // Some evaluations only happen on the master client
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            // If the current leader has died then stop following it
            if (TargetedObjectToFollow != null && TargetedObjectToFollow.tag == "Dead Enemy")
            {
                // Tell everyone that this enemy should stop following the current leader
                ObjPhotonView.RPC("ForgetLeader", PhotonTargets.All);
            }
        }

        // Units can only move while firing on a tower IF that ability has been enabled. Also, units will not move when attacking the mining facility
        if (((TargetedObjectToAttack != null && EnemyAttributes.AttackWhileMoving) || TargetedObjectToAttack == null) && TargetType != TargetType.MiningFacility && !IsStunned)
        {
            // MASTER CLIENT movement
            if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
            {
                // Use Shortest Path for movement
                if (PathFinding == PathFindingType.ShortestPath)
                {
                    // Only continue moving if we haven't yet arrived at our destination
                    if (ObjPathfinder.Next() != ObjHexLocation.location)
                    {
                        // Check to see if the pathfinder is targetting a new hex in its path
                        if (DestinationHex != ObjPathfinder.Next().Position())
                        {
                            // Tell all players (including the MASTER CLIENT) that there is a new target hex
                            ObjPhotonView.RPC("UpdateEnemyTargetHex", PhotonTargets.All, ObjPathfinder.Next().Position());
                        }

                        // Slerp the Enemy's rotation to look at the new Hex location
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Destination - transform.position, Up), Time.deltaTime * EnemyAttributes.TurningSpeed);

                        // The enemy's current velocity is always the speed (during this pathfinding) - it does not use acceleration
                        CurVelocity = transform.forward * EnemyAttributes.Speed;
                    }
                }
                // Track Friendly while Ignoring Pathfinding for movement
                else if (PathFinding == PathFindingType.TrackEnemy_IgnorePath)
                {
                    if (TargetedObjectToFollow != null)
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(TargetedObjectToFollow.transform.position - transform.position, Up), Time.deltaTime * EnemyAttributes.TurningSpeed);
                    else
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(GameManager.Instance.ObjMiningFacility.transform.position - transform.position, Up), Time.deltaTime * EnemyAttributes.TurningSpeed);

                    // Determine Acceleration
                    CurAcceleration = this.transform.forward * EnemyAttributes.Acceleration;

                    // Determine Velocity
                    CurVelocity += CurAcceleration * Time.deltaTime * Time.deltaTime * EnemyAttributes.Speed;
                    CurVelocity = Vector3.ClampMagnitude(CurVelocity, EnemyAttributes.Speed);
                }
            }
            // CLIENT movement
            else
            {
                // Use pathfinding to predict movement
                if (ObjPathfinder != null)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Destination - transform.position, Up), Time.deltaTime * EnemyAttributes.TurningSpeed);

                    // The enemy's current velocity is always the speed (during this pathfinding)
                    CurVelocity = transform.forward * EnemyAttributes.Speed;
                }
            }

            // Ensure that we don't get too close to the ground
            // NOTE: This is a kludge. In the end we should adjust the ship's yaw so that it doesn't hit the surface instead of just putting this weird upward force on it
            float minHoverDistance = EnemyAttributes.HoverDistance * 0.5f;
            var intersection = GameManager.Instance.TerrainMesh.IntersectPosition(this.transform.position);
            CurHeight = Mathf.Abs(this.transform.position.z - intersection.z);
            
			if (CurHeight < minHoverDistance)
            {
                // We're too close to the ground, apply upward velocity proportionate to how close we are
				CurVelocity.z -= (minHoverDistance - CurHeight) * EnemyAttributes.Speed * 10.0f * Time.deltaTime;
            }

            // Manually change the Enemy's position
            this.transform.position += CurVelocity * Time.deltaTime;
        }
    }

    #region IDENTIFYING TARGETS

    protected virtual void EvaluatePotentialTarget(GameObject other)
    {
        // Only run evaluation if this is the Master Client
        if (!SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            return;
        }

        // If we're already targeting a mining facility then don't bother changing the target
        if (TargetType == TargetType.MiningFacility)
        {
            return;
        }

        if (EnemyAttributes.AttackMiningFacility)
        {
            var facility = other.GetComponent<MiningFacility>();
            if (facility != null)
            {
                // Tell everyone that this enemy is targeting the mining facility
                ObjPhotonView.RPC("TargetFacility", PhotonTargets.All, facility.NetworkViewID);
                return;
            }
        }

        // If we're already targeting a tower then don't bother changing the target
        if (TargetType == TargetType.Tower)
        {
            return;
        }

        if (EnemyAttributes.AttackTowers)
        {
            var tower = other.GetComponent<Tower>();
            if (tower != null)
            {
                // Tell everyone that this enemy is targeting the tower
                ObjPhotonView.RPC("TargetTower", PhotonTargets.All, tower.NetworkViewID);
                return;
            }
        }
    }

    protected virtual void EvaluatePotentialLeader(GameObject other)
    {
        // Only run evaluation if this is the Master Client
        if (!SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            return;
        }

        // If we're already following a leader then don't change
        if (TargetedObjectToFollow != null)
        {
            return;
        }

        // Check whether we're configured to follow a leader
        if (PathFinding != PathFindingType.TrackEnemy_IgnorePath && PathFinding != PathFindingType.TrackEnemy_FollowPath)
        {
            return;
        }

        // Don't follow dead leaders
        if (other.tag == "Dead Enemy")
        {
            return;
        }

        // Check whether this is actually a fellow enemy
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null)
        {
            return;
        }

        // Only take action if the potential leader is of a different kind than us
        if (enemy.EnemyAttributes.DisplayName == this.EnemyAttributes.DisplayName)
        {
            return;
        }

        // Tell everyone that this enemy is now following a leader
        ObjPhotonView.RPC("FollowEnemy", PhotonTargets.All, enemy.NetworkViewID);
    }

    public virtual void OnTriggerStay(Collider other)
    {
        // Only run evaluation if this is the Master Client
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            EvaluatePotentialTarget(other.gameObject);
            EvaluatePotentialLeader(other.gameObject);
        }
    }

    /// <summary>
    /// Trigger event used to identify when objects exit the enemy's firing radius
    /// </summary>
    /// <param name="other">Collider definitions</param>
    protected virtual void OnTriggerExit(Collider other)
    {
        // Only the Master Client updates target data
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            // Remove the target when it leaves the enemy's range
            if (other.gameObject.Equals(TargetedObjectToAttack))
            {
                // Tell everyone that the enemy should forget its target
                ObjPhotonView.RPC("ForgetTarget", PhotonTargets.All);
            }
        }
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        // Only Target objects to follow if this is the Master Client
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            // Note from Josh 2015-11-13: I think we're using the same collider for both kamikaze range and firing range tests.

            // Note from Josh 2015-11-13: I really think this next conditional is bad. Why should the facility be damaged instantly
            // as soon as an enemy gets within firing range? Shouldn't we just let the Fire() function handle this? If this is meant
            // for the kamikaze function, then it probably needs some additional conditionals for it to work correctly.

            // Check to see if the Enemy encounters the Mining Facility and - if so - explode on impact
            if (other.tag == "Mining Facility")
            {
                GameManager.Instance.ObjMiningFacility.TakeDamage(EnemyAttributes.BallisticDamage, EnemyAttributes.ThraceiumDamage);
            }
        }
    }

    #endregion IDENTIFYING TARGETS

    #region RPC CALLS

    /// <summary>
    /// Tells the client to target a tower
    /// </summary>
    /// <param name="viewID">NetworkViewID of the enemy to target.</param>
    [PunRPC]
    protected virtual void TargetTower(int viewID)
    {
        var tower = GameManager.Instance.TowerManager.FindTowerByID(viewID);
        if (tower == null)
        {
            LogError(string.Format("Enemy[{0}] told to target tower[{1}] which doesn't exist", NetworkViewID, viewID));
            return;
        }
        else
        {
            Log(string.Format("Enemy[{0}] targets tower[{1}]", NetworkViewID, viewID));
        }

        TargetedObjectToAttack = tower.gameObject;
    }

    /// <summary>
    /// Tells the enemy to target a facility.
    /// </summary>
    /// <param name="viewID">NetworkViewID of the mining facility to target.</param>
    [PunRPC]
    protected virtual void TargetFacility(int viewID)
    {
        var facility = GameManager.Instance.ObjMiningFacility;
        if (facility == null || facility.NetworkViewID != viewID)
        {
            LogError(string.Format("Enemy[{0}] told to target facility[{1}] which doesn't exist", NetworkViewID, viewID));
            return;
        }
        else
        {
            Log(string.Format("Enemy[{0}] targets facility[{1}]", NetworkViewID, viewID));
        }

        TargetedObjectToAttack = facility.gameObject;
    }

    /// <summary>
    /// Tells the enemy to forget its target.
    /// </summary>
    [PunRPC]
    protected virtual void ForgetTarget()
    {
        Log(string.Format("Enemy[{0}] forgets its target", NetworkViewID));
        TargetedObjectToAttack = null;
    }

    /// <summary>
    /// Tells the enemy to follow another enemy.
    /// </summary>
    /// <param name="viewID">NetworkViewID of the enemy to follow.</param>
    [PunRPC]
    protected virtual void FollowEnemy(int viewID)
    {
        var enemy = GameManager.Instance.EnemyManager.FindEnemyByID(viewID);
        if (enemy == null)
        {
            LogError(string.Format("Enemy[{0}] told to follow enemy[{1}] which doesn't exist", NetworkViewID, viewID));
            return;
        }
        else
        {
            Log(string.Format("Enemy[{0}] follows enemy[{1}]", NetworkViewID, viewID));
        }

        TargetedObjectToFollow = enemy.gameObject;
    }

    /// <summary>
    /// Tells the enemy to follow another enemy.
    /// </summary>
    /// <param name="viewID">NetworkViewID of the enemy to follow. Pass -1 to set the target to null.</param>
    [PunRPC]
    protected virtual void ForgetLeader()
    {
        Log(string.Format("Enemy[{0}] forgets its leader", NetworkViewID));
        TargetedObjectToFollow = null;
    }

    /// <summary>
    /// RPC call to tell players to fire a shot
    /// </summary>
    [PunRPC]
    protected virtual void FireAcrossNetwork()
    {
        // Tell object to take damage
        switch (TargetType)
        {
            case TargetType.MiningFacility:
                Log(string.Format("Enemy[{0}] fires at mining facility", NetworkViewID));
                var facility = TargetedObjectToAttack.GetComponent<MiningFacility>();
                if (facility != null)
                {
                    facility.TakeDamage(EnemyAttributes.BallisticDamage, EnemyAttributes.ThraceiumDamage);
                }
                break;
            case TargetType.Tower:
                Log(string.Format("Enemy[{0}] fires at tower", NetworkViewID));
                var tower = TargetedObjectToAttack.GetComponent<Tower>();
                if (tower != null)
                {
                    tower.TakeDamage(EnemyAttributes.BallisticDamage, EnemyAttributes.ThraceiumDamage);
                }
                break;
        }

        if (GameManager.Instance.GameRunning)
            AnalyticsAsset.AddDamageDealt(EnemyAttributes.BallisticDamage, EnemyAttributes.ThraceiumDamage);

        // Reset timer for tracking when to fire next
        TimeLastShotFired = Time.time;

        InstantiateFire();
        InstantiateExplosion(); // TODO: Delay?
    }

    /// <summary>
    /// RPC Call to tell players the enemy needs to take damage
    /// </summary>
    /// <param name="ballisticsDamage">Ballistics damage</param>
    /// <param name="thraceiumDamage">Thraceium damage</param>
    [PunRPC]
    protected virtual void TakeDamageAcrossNetwork(float ballisticDamage, float thraceiumDamage)
    {
        // Damage dealt after defense is calculated
        float bDamageWithDefense = (ballisticDamage * (1 - EnemyAttributes.BallisticDefense));
        float tDamageWithDefense = (thraceiumDamage * (1 - EnemyAttributes.ThraceiumDefense));

        // Take damage from Ballistics and Thraceium
        Health -= bDamageWithDefense;
        Health -= tDamageWithDefense;
        Health = Mathf.Max(Health, 0);

        // Send specific asset's Ballisitic and Therceium Damage (w/o defense) to AnalyticsManager
        if (GameManager.Instance.GameRunning)
            AnalyticsAsset.AddDamageTaken(ballisticDamage, thraceiumDamage);

        // Only update the Health Bar if there is one to update
        if (HealthBar)
            HealthBar.UpdateHealthBar(Health);

        // Display damage taken above the enemy
        ShowPopUpDamage(ballisticDamage + thraceiumDamage);
    }

    /// <summary>
    /// RPC Call to tell players to kill the enemy
    /// </summary>
    [PunRPC]
    protected virtual void DieAcrossNetwork()
    {
        // Indicate to the AnalyticsManager where the Enemy has died
        if (GameManager.Instance.GameRunning)    
            AnalyticsAsset.DeathOfAsset(transform.position);

        // Simply destroy the enemy (show explosion and play sound)
        DestroyEnemy();
    }

    /// <summary>
    /// CLIENT: Tells the client the next hex to enemy is traveling towards
    /// </summary>
    /// <param name="newTargetHex">New target hex.</param>
    [PunRPC]
    protected void UpdateEnemyTargetHex(Vector2 newTargetHex)
    {
        DestinationHex = newTargetHex;
    }

    /// <summary>
    /// RPC call to tell players the enemy is indefinitely stunned.
    /// </summary>
    [PunRPC]
    public virtual void StunnedAcrossNetwork(bool isStunned)
    {
        IsStunned = isStunned;
    }

    /// <summary>
    /// RPC call to tell players the enemy is stunned for a length of time.
    /// </summary>
    [PunRPC]
    public virtual void StunnedAcrossNetwork(float stunDuration)
    {
        IsStunned = true; // redundancy
        StunEndTime = Time.time + stunDuration;
    }

    #endregion RPC CALLS

    #region TAKE DAMAGE / DIE / STUN

    public virtual void TakeDamage(float ballisticDamage, float thraceiumDamage, PhotonPlayer towerOwner)
    {
        // Only the master client dictates how to handle damage
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            // Make sure the enemy isn't already dead.
            ///  Is this really needed? - BS 8/28/15
            if (Health >= 0)
            {
                // Damage dealt after defense is calculated
                float bDamageWithDefense = (ballisticDamage * (1 - EnemyAttributes.BallisticDefense));
                float tDamageWithDefense = (thraceiumDamage * (1 - EnemyAttributes.ThraceiumDefense));

                // Take damage from Ballistics and Thraceium
                Health -= bDamageWithDefense;
                Health -= tDamageWithDefense;
                Health = Mathf.Max(Health, 0);

                // Send specific asset's Ballisitic and Therceium Damage (w/o defense) to AnalyticsManager
                if (GameManager.Instance.GameRunning)
                    AnalyticsAsset.AddDamageTaken(ballisticDamage, thraceiumDamage);

                // Only update the Health Bar if there is one to update
                if (HealthBar)
                    HealthBar.UpdateHealthBar(Health);

                // Display damage taken above the enemy
                ShowPopUpDamage(ballisticDamage + thraceiumDamage);

                // Tell the player whose tower made the shot that it was a hit!
                PlayerManager.Instance.InformPlayerOfDamagedEnemy(towerOwner, EnemyAttributes.DisplayName, thraceiumDamage, ballisticDamage, Health <= 0);

                // Either tell all other clients the enemy is dead, or tell them to have the enemy take damage
                if (Health <= 0)
                {
                    ObjPhotonView.RPC("DieAcrossNetwork", PhotonTargets.All, null);
                }
                else
                    ObjPhotonView.RPC("TakeDamageAcrossNetwork", PhotonTargets.Others, ballisticDamage, thraceiumDamage);
            }
        }
    }

    /// <summary>
    /// Display damage taken above the enemy
    /// </summary>
    public void ShowPopUpDamage(float totalDamage)
    {
        PopUp<Enemy> popUp = new PopUp<Enemy>(this.gameObject, totalDamage, 0.5f);
    }

    /// <summary>
    /// Forces the Enemy to die regardless of how much health is left
    /// </summary>
    public virtual void ForceInstantDeath()
    {
        // Indicate to the AnalyticsManager where the Enemy has died
        if (GameManager.Instance.GameRunning)    
            AnalyticsAsset.DeathOfAsset(transform.position);

        // If this is the master client then they tell all other clients to destroy this enemy
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
            ObjPhotonView.RPC("DieAcrossNetwork", PhotonTargets.All, null);
    }

    /// <summary>
    /// Destroy the Enemy from all areas of the game
    /// </summary>
    protected virtual void DestroyEnemy()
    {
        // Instantiate a prefab containing an FMOD_OneShot of enemy's explosion sound.
        if (ExplodingEffect)
            Instantiate(ExplodingEffect, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), this.transform.rotation);

        // Tell the enemy manager this enemy is being destroyed
        GameManager.Instance.EnemyManager.RemoveActiveEnemy(this);

        // The GameObject must be destroyed or else the enemy will stay instantiated
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Stun the enemy indefinitely.
    /// </summary>
    public virtual void Stunned(bool stunCondition)
    {
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            ObjPhotonView.RPC("StunnedAcrossNetwork", PhotonTargets.All, stunCondition);
        }
    }

    /// <summary>
    /// Stun the enemy for an alloted amount of time.
    /// </summary>
    public virtual void Stunned(float stunDuration)
    {
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            ObjPhotonView.RPC("StunnedAcrossNetwork", PhotonTargets.All, stunDuration);
        }
    }

    #endregion TAKE DAMAGE / DIE / STUN

    #region ATTACKING

    /// <summary>
    /// Coroutine that constantly checks to see if tower is ready to fire upon an enemy
    /// </summary>
    protected virtual IEnumerator Fire()
    {
        // Infinite Loop FTW
        while (true)
        {
            // Only fire if the enemy isn't stunned.
            if (!IsStunned)
            {
                // Only perform the act of firing if this is the Master Client
                if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
                {
                    // Only fire if there is an enemy being targeted
                    if (TargetedObjectToAttack != null)
                    {
                        // FIXME: Only fire if in range?

                        // Only fire if enemy is ready to fire
                        if (Time.time - TimeLastShotFired >= (1 / EnemyAttributes.RateOfFire))
                        {
                            // Only fire if the enemy (or its turret) is facing the target

                            // TODO: Use a variable angle limit specified in enemy data?
                            // FIXME: Figure out why the following angle calculation doesn't work

                            //Transform emission = (EmissionPoint != null) ? EmissionPoint.transform : this.transform;
                            //float angle = Vector3.Angle(emission.forward, TargetedObjectToAttack.transform.position - emission.position);
                            float angle = 0.0f;
                            if (angle <= 8.0f)
                            {
                                // Tell all clients that this enemy is firing at its target
                                ObjPhotonView.RPC("FireAcrossNetwork", PhotonTargets.All);
                            }
                        }
                    }
                }
            }

            yield return 0;
        }
    }

    #endregion ATTACKING

    #region Special Effects

    public virtual List<GameObject> InstantiateFire()
    {
        // Instantiate prefabs for firing a shot
        List<GameObject> effects = new List<GameObject>();
        if (FiringEffect)
        {
            GameObject effect;
            List<GameObject> emitters;
            if (FireEffectEmitters != null && FireEffectEmitters.Length > 0)
            {
                emitters = new List<GameObject>(FireEffectEmitters);
            }
            else
            {
                emitters = new List<GameObject>(1);
                emitters.Add(this.gameObject);
            }
            foreach (var emitter in emitters)
            {
                effect = Instantiate(FiringEffect, emitter.transform.position, emitter.transform.rotation) as GameObject;
                if (TargetedObjectToAttack != null)
                {
                    effect.transform.LookAt(TargetedObjectToAttack.transform.position);
                    var laser = effect.GetComponent<LaserFire>();
                    if (laser != null)
                    {
                        laser.Target = TargetedObjectToAttack.transform;
                    }
                }
                effects.Add(effect);
            }
        }
        return effects;
    }

    public virtual void InstantiateExplosion()
    {
        if (ExplodingEffect && TargetedObjectToAttack)
        {
            Instantiate(ExplodingEffect, TargetedObjectToAttack.transform.position, TargetedObjectToAttack.transform.rotation);
        }
    }

    #endregion Special Effects

    #region MessageHandling

    protected virtual void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[Enemy] " + message);
    }

    protected virtual void LogError(string message)
    {
        Debug.LogError("[Enemy] " + message);
    }

    #endregion MessageHandling
}