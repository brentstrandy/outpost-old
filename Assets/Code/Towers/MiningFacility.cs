using UnityEngine;

public class MiningFacility : Tower
{
    private static MiningFacility instance;

    public float IncomePerSecond { get; private set; }

    private float LastIncomeTime;

    // Use this for initialization
    public override void Start()
    {
        IncomePerSecond = 1.0f;
        LastIncomeTime = Time.time;
        Health = 100.0f;

        // Save reference to PhotonView
        ObjPhotonView = PhotonView.Get(this);
        NetworkViewID = ObjPhotonView.viewID;
    }

    public void InitializeFromLevelData(LevelData levelData)
    {
        // Track the newly added tower in the TowerManager
        GameManager.Instance.TowerManager.AddActiveTower(this);

        IncomePerSecond = levelData.IncomePerSecond;
        Health = levelData.MiningFacilityHealth;
        PlayerManager.Instance.CurPlayer.SetStartingMoney(levelData.StartingMoney);

        // Lift the facility up to whatever the height of the ground below it is
        gameObject.transform.position = GameManager.Instance.TerrainMesh.IntersectPosition(gameObject.transform.position);

        // Send the tower's viewID and spawn coordinate to AnalyticsManager
        AnalyticsManager.Instance.Assets.AddAsset("Tower", "Mining Facility", NetworkViewID, gameObject.transform.position);

        // Store a reference to the AnalyticsManager's information on this Tower
        AnalyticsAsset = AnalyticsManager.Instance.Assets.FindAsset("Tower", "Mining Facility", NetworkViewID);
    }

    // Update is called once per frame
    public override void Update()
    {
        EarnIncome();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            // Deal damage to the Mining Facility
            this.TakeDamage(other.gameObject.GetComponent<Enemy>().EnemyAttributes.BallisticDamage, other.gameObject.GetComponent<Enemy>().EnemyAttributes.ThraceiumDamage);
            // Force Enemy to kill itself
            other.gameObject.GetComponent<Enemy>().ForceInstantDeath();
        }
    }

    protected override void OnTriggerStay(Collider other)
    {
    }

    protected override void OnTriggerExit(Collider other)
    {
    }

    private void EarnIncome()
    {
        // Only earn income if enough time has passed
        if (Time.time - LastIncomeTime >= 1)
        {
            PlayerManager.Instance.CurPlayer.EarnIncome(IncomePerSecond);
            LastIncomeTime = Time.time;
        }
    }
}