using System.Collections.Generic;
using UnityEngine;
using MGUtilities;
using System.IO;
using UnityEngine.InputSystem;

public class Tower : Singleton_template<Tower>
{
    [SerializeField] private Turret m_turret;
    [SerializeField] private float m_baseRange;
    [SerializeField] private LayerMask m_targetlayers;

    private Vector2 m_target;
    private Transform m_transform;
    private DamageableObject m_damageableObject;

    private readonly List<TargetInfo> m_targets = new();

    private string m_path;
    public bool IsDead { get { return m_damageableObject.IsDead; } }
    public int Penetration { get; set; }
    public float BulletSpeed { get; set; }
    public float Damage { get; set; }
    public float RotateSpeed { get; set; }
    public float FireRate { get; set; }
    public Turret CurrentTurret { get { return m_turret; } set 
        {
            m_turret.gameObject.SetActive(false);
            m_turret = value;
            m_turret.gameObject.SetActive(true);
        } }
    protected override void Awake()
    {
        base.Awake();
        m_transform = transform;
        m_damageableObject = GetComponent<DamageableObject>();
        m_path = Path.Combine(Application.dataPath, "Stats.txt");
        if (!File.Exists(m_path))
        {
            string defaultContent = $"{0},{1},{1},{1},{1}";
            File.WriteAllText(m_path, defaultContent);
        }
        ReadStats();
        UpdateTurretStats();
    }
    public void UpdateTurretStats()
    {
        m_turret.UpdateStats(Penetration, BulletSpeed, Damage, RotateSpeed, FireRate);
    }
    public void DamageTower(float d)
    {
        m_damageableObject.TakeDamage(d);
    }
    void Update()
    {
        if (m_damageableObject.IsDead)
        {
            GameOver();
            return;
        }
        m_damageableObject.RunRegen();

        UpdateTargetsInRange();

        if (m_targets.Count == 0)
            return;

        TargetInfo bestTarget = GetBestTarget();
        if (bestTarget != null)
        {
            Enemy enemy = bestTarget.m_transform.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                Vector2 predictedPos = bestTarget.m_transform.position + enemy.Velocity * Time.deltaTime;
                m_turret.RotateTurret(predictedPos);
                m_turret.Shoot();
            }
        }
    }
    private void GameOver()
    {
        string content = $"{Penetration},{BulletSpeed},{Damage},{RotateSpeed},{FireRate}";
        File.WriteAllText(m_path, content);
    }
    private void ReadStats()
    {
        string content = File.ReadAllText(m_path);
        string[] values = content.Split(',');
        if (values.Length >= 5)
        {
            Penetration = int.Parse(values[0]);
            BulletSpeed = float.Parse(values[1]);
            Damage = float.Parse(values[2]);
            RotateSpeed = float.Parse(values[3]);
            FireRate = float.Parse(values[4]);
        }
        else Debug.LogWarning("Invalid data in file: not enough values.");
    }
    private void UpdateTargetsInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(m_transform.position, m_baseRange, m_targetlayers);
        HashSet<Transform> seenThisFrame = new();

        foreach (var hit in hits)
        {
            Transform tf = hit.transform;
            DamageableObject dmg = hit.GetComponentInParent<DamageableObject>();
            if (dmg == null) continue;

            seenThisFrame.Add(tf);

            TargetInfo existing = m_targets.Find(t => t.m_transform == tf);
            if (existing != null)
            {
                existing.m_lastSeenTime = Time.time;
            }
            else
            {
                m_targets.Add(new TargetInfo
                {
                    m_transform = tf,
                    m_dmgObj = dmg,
                    m_lastSeenTime = Time.time
                });
            }
        }

        // Remove any no longer in range
        m_targets.RemoveAll(t => !seenThisFrame.Contains(t.m_transform));
    }

    private TargetInfo GetBestTarget()
    {
        Vector3 turretPos = m_transform.position;
        Vector2 turretForward = m_turret.transform.up;

        m_targets.Sort((a, b) =>
        {
            float da = Vector2.Distance(turretPos, a.m_transform.position);
            float db = Vector2.Distance(turretPos, b.m_transform.position);

            if (Mathf.Abs(da - db) > 0.5f) return da.CompareTo(db); // closest wins

            float hpa = a.m_dmgObj.CurrentHp;
            float hpb = b.m_dmgObj.CurrentHp;

            if (!Mathf.Approximately(hpa, hpb)) return hpa.CompareTo(hpb); // lower HP wins

            float angleA = Vector2.Angle(turretForward, (a.m_transform.position - turretPos).normalized);
            float angleB = Vector2.Angle(turretForward, (b.m_transform.position - turretPos).normalized);

            if (!Mathf.Approximately(angleA, angleB)) return angleA.CompareTo(angleB); // smallest angle wins

            return a.m_lastSeenTime.CompareTo(b.m_lastSeenTime); // older target wins
        });

        return m_targets[0];
    }
}
public class TargetInfo
{
    public Transform m_transform;
    public DamageableObject m_dmgObj;
    public float m_lastSeenTime;
}
