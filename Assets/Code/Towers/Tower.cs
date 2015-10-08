using Settworks.Hexagons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HexLocation))]
public class Tower : MonoBehaviour
{
    protected readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);

    public bool ShowDebugLogs = true;
    public PhotonPlayer Owner { get; protected set; }

    // Tower Attributes/Details/Data
    public TowerData TowerAttributes;

    public float Health { get; protected set; }
    public int NetworkViewID { get; protected set; }

    public Color PlayerColor { get; private set; }
    protected Enemy TargetedEnemy = null;
    protected bool CanFire = false;
    protected bool ReadyToFire = true;
    protected bool CanMove = false;

    public GameObject TurretPivot;
    public GameObject EmissionPoint;
    public GameObject TowerRing;

    // Components
    public HealthBarController HealthBar;

    protected PhotonView ObjPhotonView;
    protected HexLocation ObjHexLocation;
    protected Animator ObjAnimator;

    // Effects
    public GameObject FiringEffect;

    public GameObject ExplodingEffect;

    #region INITIALIZE

    public void Awake()
    {
        ObjHexLocation = GetComponent<HexLocation>();
        ObjAnimator = GetComponent<Animator>();
    }

    // Use this for initialization
    public virtual void Start()
    {
        // Update the hex coordinate to reflect the spawned position
        ObjHexLocation.ApplyPosition();

        // Track the newly added tower in the TowerManager
        GameManager.Instance.TowerManager.AddActiveTower(this);

        // If this tower has range attack, inform the tower manager of our coverage area
        if (TowerAttributes.Range > 0)
        {
            GameManager.Instance.TowerManager.AddCoverage(this);
        }

        if (TowerRing != null)
        {
            TowerRing.transform.localScale *= TowerAttributes.AdjustedRange;

            bool selected = (PlayerManager.Instance.SelectedTowerCoord == ObjHexLocation.location);
            if (selected)
            {
                OnSelect();
            }
            else
            {
                OnDeselect();
            }
        }
    }

    /// <summary>
    /// Sets the tower's properties based on TowerData
    /// </summary>
    /// <param name="towerData">Tower data</param>
    public virtual void SetTowerData(TowerData towerData, PhotonPlayer owner)
    {
        TowerAttributes = towerData;
        Owner = owner;
        Health = TowerAttributes.MaxHealth;
        PlayerColor = PlayerColors.colors[(int)Owner.customProperties["PlayerColorIndex"]];

        // Set the speed at which this tower is built
        //this.GetComponent<Animation>()[TowerAttributes.PrefabName + "_Build"].speed = (1 / TowerAttributes.StartupTime);

        ObjPhotonView = PhotonView.Get(this);
        NetworkViewID = ObjPhotonView.viewID;

        // Make the Master Client the owner of this object (authoritative server)
        ObjPhotonView.TransferOwnership(SessionManager.Instance.GetMasterClientID());

        // If the tower has range attack, add a sphere collider to detect range
        if (TowerAttributes.Range > 0)
        {
            GameObject go = new GameObject("Awareness", typeof(SphereCollider));
            go.GetComponent<SphereCollider>().isTrigger = true;
            go.GetComponent<SphereCollider>().radius = TowerAttributes.Range * 2; // Question from Josh: Why are we multiplying by 2 here?
            go.transform.parent = this.transform;
            go.transform.localPosition = Vector3.zero;
        }

        // Set the player's color on any material titled "PlayerColorMaterial"
        foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
        {
            foreach (Material material in renderer.materials)
            {
                if (material.name.Contains("PlayerColorMaterial"))
                {
                    material.SetColor("_Color", PlayerColor);
                    material.SetColor("_EmissionColor", PlayerColor);
                }
            }
        }

        // Initiate animation playback speeds based on Tower Attributes
        ObjAnimator.SetFloat("Cooldown Playback Speed", TowerAttributes.RateOfFire);

        // Only initialize the health bar if it is used for this tower
        if (HealthBar)
            HealthBar.InitializeBars(TowerAttributes.MaxHealth);
    }

    #endregion INITIALIZE

    // Update is called once per frame
    public virtual void Update()
    {
        // Perform actions if the tower is targeting an enemy
        if (CanMove && TargetedEnemy)
        {
            // Have the tower's pivot point look at the targeted enemy
            if (TurretPivot)
            {
                TurretPivot.transform.rotation = Quaternion.Slerp(TurretPivot.transform.rotation, Quaternion.LookRotation(TargetedEnemy.transform.position - TurretPivot.transform.position, Up), Time.deltaTime * TowerAttributes.TrackingSpeed);
            }
        }
    }

    /// <summary>
    /// Determines whether this tower has full health.
    /// </summary>
    /// <returns><c>true</c> if this tower has full health; otherwise, <c>false</c>.</returns>
    public bool HasFullHealth()
    {
        return Health >= TowerAttributes.MaxHealth;
    }

    public virtual IEnumerable<HexCoord> Coverage()
    {
        HexCoord origin = ObjHexLocation.location;
        int approxHexRange = (int)(TowerAttributes.Range * 3.0f);
        Predicate<HexCoord> obstacles = (HexCoord coord) => { return false; };
        float range = TowerAttributes.Range * 2.0f + 1.0f; // Double range to match sphere collider and then add radius
        foreach (var node in HexKit.Spread(origin, approxHexRange, obstacles))
        {
            if (Vector2.Distance(origin.Position(), node.Location.Position()) <= range)
            {
                yield return node.Location;
            }
        }
    }

    /// <summary>
    /// Used by the animation system. Called when the Startup Animation Finishes
    /// </summary>
    public virtual void OnBuildAnimFinished()
    {
        CanFire = true;
        CanMove = true;
    }

    public virtual void OnCooldownAnimFinished()
    {
        ReadyToFire = true;
    }

    #region IDENTIFYING TARGETS

    /// <summary>
    /// Trigger event used to identify when Enemies enter the tower's firing radius
    /// </summary>
    /// <param name="other">Collider definitions</param>
    protected virtual void OnTriggerStay(Collider other)
    {
        // Only Target enemies if this is the Master Client
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            // Only target a new enemy if an enemy isn't already targeted
            if (TargetedEnemy == null)
            {
                // Only target Enemy game objects
                if (other.tag == "Enemy")
                {
                    // Tell all other clients to target a new enemy
                    ObjPhotonView.RPC("TargetNewEnemy", PhotonTargets.Others, other.GetComponent<Enemy>().NetworkViewID);

                    // Master Client targets the Enemy
                    TargetedEnemy = other.gameObject.GetComponent<Enemy>();
                }
            }
            // Check to see if the enemy has died
            else if (TargetedEnemy.tag == "Dead Enemy")
            {
                // Tell all other clients that this tower is no longer targeting the dead enemy
                ObjPhotonView.RPC("TargetNewEnemy", PhotonTargets.Others, -1);

                TargetedEnemy = null;
            }
        }
    }

    /// <summary>
    /// Trigger event used to identify when Enemies exit the tower's firing radius
    /// </summary>
    /// <param name="other">Collider definitions</param>
    protected virtual void OnTriggerExit(Collider other)
    {
        // Only Target enemies if this is the Master Client
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            // Only reset the target enemy if one is already targeted
            if (TargetedEnemy != null)
            {
                // Only reset the target if the object exiting is an enemy
                if (other.tag == "Enemy")
                {
                    // Remove the targeted Enemy when the enemy leaves
                    if (other.gameObject.GetComponent<Enemy>().Equals(TargetedEnemy))
                    {
                        // Tell all other clients that
                        ObjPhotonView.RPC("TargetNewEnemy", PhotonTargets.Others, -1);

                        TargetedEnemy = null;
                    }
                }
            }
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
    }

    #endregion IDENTIFYING TARGETS

    #region RPC CALLS

    /// <summary>
    /// Tells the client to target a new enemy
    /// </summary>
    /// <param name="viewID">Network ViewID of the enemy to target. Pass -1 to set the target to null.</param>
    [PunRPC]
    protected virtual void TargetNewEnemy(int viewID)
    {
        // A viewID of -1 means there is no targeted enemy. Otherwise, find the enemy by the networkViewID
        if (viewID == -1)
            TargetedEnemy = null;
        else
            TargetedEnemy = GameManager.Instance.EnemyManager.FindEnemyByID(viewID);
    }

    /// <summary>
    /// RPC call to tell players to fire a shot
    /// </summary>
    [PunRPC]
    protected virtual void FireAcrossNetwork()
    {
        // Tell enemy to take damage (only the Master Client can do this)
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
            TargetedEnemy.TakeDamage(TowerAttributes.BallisticDamage, TowerAttributes.ThraceiumDamage, Owner);

        // Tell the tower Animator the tower has fired
        ObjAnimator.SetTrigger("Shot Fired");

        // After the tower fires it loses the ability to fire again until after a cooldown period
        ReadyToFire = false;

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
        float bDamageWithDefense = (ballisticDamage * (1 - TowerAttributes.BallisticDefense));
        float tDamageWithDefense = (thraceiumDamage * (1 - TowerAttributes.ThraceiumDefense));

        // Take damage from Ballistics and Thraceium
        Health -= bDamageWithDefense;
        Health -= tDamageWithDefense;
        Health = Mathf.Max(Health, 0);

        AnalyticsManager.Instance.BallisticDamage += bDamageWithDefense;
        AnalyticsManager.Instance.ThraceiumDamage += tDamageWithDefense;

        // Only update the Health Bar if there is one to update
        if (HealthBar)
            HealthBar.UpdateHealthBar(Health);

        // Display health taken above target
        ShowPopUpDamage(bDamageWithDefense + tDamageWithDefense);
    }

    /// <summary>
    /// RPC Call to tell players to kill the enemy
    /// </summary>
    [PunRPC]
    protected virtual void DieAcrossNetwork()
    {
        // Simply destroy the tower (show explosion and play sound)
        DestroyTower();
    }

    #endregion RPC CALLS

    #region TAKE DAMAGE / DIE / HEAL

    /// <summary>
    /// Tower takes damage and responds accordingly
    /// </summary>
    /// <param name="damage">Damage.</param>
    public virtual void TakeDamage(float ballisticDamage, float thraceiumDamage)
    {
        // Only the master client dictates how to handle damage
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
        {
            // Damage dealt after defense is calculated
            float bDamageWithDefense = (ballisticDamage * (1 - TowerAttributes.BallisticDefense));
            float tDamageWithDefense = (thraceiumDamage * (1 - TowerAttributes.ThraceiumDefense));

            // Take damage from Ballistics and Thraceium
            Health -= bDamageWithDefense;
            Health -= tDamageWithDefense;
            Health = Mathf.Max(Health, 0);

            AnalyticsManager.Instance.BallisticDamage += bDamageWithDefense;
            AnalyticsManager.Instance.ThraceiumDamage += tDamageWithDefense;

            // Only update the Health Bar if there is one to update
            if (HealthBar)
                HealthBar.UpdateHealthBar(Health);

            // Display health taken above target
            ShowPopUpDamage(bDamageWithDefense + tDamageWithDefense);

            // Either tell all other clients the enemy is dead, or tell them to have the enemy take damage
            if (Health <= 0)
                ObjPhotonView.RPC("DieAcrossNetwork", PhotonTargets.All, null);
            else
                ObjPhotonView.RPC("TakeDamageAcrossNetwork", PhotonTargets.Others, ballisticDamage, thraceiumDamage);
        }
    }

    /// <summary>
    /// Display damage taken above the enemy
    /// </summary>
    public void ShowPopUpDamage(float totalDamage)
    {
        PopUp<Tower> popUp = new PopUp<Tower>(this.gameObject, totalDamage, 0.5f);
    }

    /// <summary>
    /// Display health gained above the tower
    /// </summary>
    public void ShowPopUpHeal(float totalHeal)
    {
        PopUp<ThraceiumHealingTower> popUp = new PopUp<ThraceiumHealingTower>(this.gameObject, totalHeal, 0.5f);
    }

    /// <summary>
    /// Destroy the Tower from all areas of the game
    /// </summary>
    protected virtual void DestroyTower()
    {
        // Instantiate a prefab to show the tower exploding
        if (ExplodingEffect)
            Instantiate(ExplodingEffect, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), this.transform.rotation);

        // Inform the player that a tower has been destroyed
        NotificationManager.Instance.DisplayNotification(new NotificationData("", Owner.name + " lost a tower (" + this.TowerAttributes.DisplayName + ")", "QuickInfo"));

        // Tell the tower manager this tower is being destroyed
        GameManager.Instance.TowerManager.RemoveActiveTower(this);

        // Tell the tower manager to remove our coverage area
        if (TowerAttributes.Range > 0)
        {
            GameManager.Instance.TowerManager.RemoveCoverage(this);
        }

        // The GameObject must be destroyed or else the enemy will stay instantiated
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Heal the tower the specified amount.
    /// </summary>
    /// <param name="healAmount">Heal amount.</param>
    public virtual void Heal(float healAmount)
    {
        // Only allow the tower to be healed to its max health
        Health = Mathf.Min(Health + healAmount, TowerAttributes.MaxHealth);

        //// Display health gained above target
        //ShowPopUpHeal(healAmount);

        // Only update the Health Bar if there is one to update
        if (HealthBar)
            HealthBar.UpdateHealthBar(Health);
    }

    #endregion TAKE DAMAGE / DIE / HEAL

    #region ATTACKING

    /// <summary>
    /// Coroutine that constantly checks to see if tower is ready to fire upon an enemy
    /// </summary>
    protected virtual IEnumerator Fire()
    {
        // Infinite Loop FTW
        while (this)
        {
            // Only fire if the tower can fire
            if (CanFire)
            {
                // Only perform the act of firing if this is the Master Client
                if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
                {
                    // Only fire if there is an enemy being targeted
                    if (TargetedEnemy != null)
                    {
                        // Only fire if tower is ready to fire
                        if (ReadyToFire)
                        {
                            // Only fire if the tower is facing the enemy (or if the tower does not need to face the enemy)
                            if (TurretPivot == null || Vector3.Angle(TurretPivot.transform.forward, TargetedEnemy.transform.position - TurretPivot.transform.position) <= 8)
                            {
                                // Tell all clients to fire upon the enemy
                                ObjPhotonView.RPC("FireAcrossNetwork", PhotonTargets.All, null);
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

    public virtual void InstantiateFire()
    {
        // Instantiate prefab for firing a shot
        if (FiringEffect)
        {
            GameObject effect;
            if (EmissionPoint)
            {
                effect = Instantiate(FiringEffect, EmissionPoint.transform.position, EmissionPoint.transform.rotation) as GameObject;
            }
            else
            {
                effect = Instantiate(FiringEffect, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - 1.32f), this.transform.rotation) as GameObject;
            }
            effect.transform.LookAt(TargetedEnemy.gameObject.transform.position);
        }
    }

    public virtual void InstantiateExplosion()
    {
        if (ExplodingEffect)
        {
            Instantiate(ExplodingEffect, TargetedEnemy.transform.position, TargetedEnemy.transform.rotation);
        }
    }

    #endregion Special Effects

    #region UI

    public virtual void OnSelect()
    {
        //Log("Selected Tower");
        if (TowerRing != null)
        {
            TowerRing.SetActive(true);
        }
    }

    public virtual void OnDeselect()
    {
        //Log("Deselected Tower");
        if (TowerRing != null)
        {
            TowerRing.SetActive(false);
        }
    }

    #endregion UI

    #region MESSAGE HANDLING

    protected virtual void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[Tower] " + message);
    }

    protected virtual void LogError(string message)
    {
        Debug.LogError("[Tower] " + message);
    }

    #endregion MESSAGE HANDLING
}