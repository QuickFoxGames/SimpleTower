using UnityEngine;
using MGUtilities;
public class Turret : MonoBehaviour
{
    [Header("Penetration")]
    [SerializeField] private int m_basePenetration;
    [SerializeField] private int m_maxPenetration;
    private int m_penetration;
    public bool MaxPenetration { get; private set; }
    [Header("Bullet Speed")]
    [SerializeField] private float m_baseBulletSpeed;
    [SerializeField] private float m_maxBulletSpeed;
    private float m_bulletSpeed;
    public bool MaxBulletSpeed { get; private set; }
    [Header("Damage")]
    [SerializeField] private float m_baseDamage;
    [SerializeField] private float m_maxDamage;
    private float m_damage;
    public bool MaxDamage { get; private set; }
    [Header("RotateSpeed")]
    [SerializeField] private float m_baseRotateSpeed;
    [SerializeField] private float m_maxRotateSpeed;
    private float m_rotateSpeed;
    public bool MaxRotateSpeed { get; private set; }
    [Header("FireRate")]
    [SerializeField] private float m_baseFireRate;
    [SerializeField] private float m_maxFireRate;
    private float m_fireRate;
    public bool MaxFireRate { get; private set; }
    [Header("Barrels")]
    [SerializeField] private Transform[] m_bulletSpawns;
    [Header("SFX")]
    [SerializeField] private float m_volume;
    [SerializeField] private AudioClip m_clip;

    private bool m_canShoot = true;
    private Transform m_transform;
    private BulletPool m_bulletPool;
    private AudioPool m_audioPool;
    public (int, float, float, float, float) Stats { get { return (m_penetration, m_bulletSpeed, m_damage, m_rotateSpeed, m_fireRate); } }
    private void Awake()
    {
        m_transform = transform;
        m_audioPool = AudioPool.Instance();

        m_penetration = m_basePenetration;
        m_bulletSpeed = m_baseBulletSpeed;
        m_damage = m_baseDamage;
        m_rotateSpeed = m_baseRotateSpeed;
        m_fireRate = m_baseFireRate;

        MaxPenetration = false;
        MaxBulletSpeed = false;
        MaxDamage = false;
        MaxRotateSpeed = false;
        MaxFireRate = false;
    }
    private void Start()
    {
        m_bulletPool = BulletPool.Instance();
    }
    public void Shoot()
    {
        if (!m_canShoot) return;
        foreach (Transform t in m_bulletSpawns)
        {
            Bullet b = m_bulletPool.GetBullet(t.position, t.rotation);
            b._penetration = m_penetration;
            b._damage = m_damage;
            b._rb.linearVelocity = t.up * m_bulletSpeed;
            m_audioPool.SpawnAudioSource(m_transform.position, m_clip, m_volume);
        }
        StartCoroutine(Coroutines.DelayBoolChange(false, true, 1f / (m_fireRate / 60f) / m_bulletSpawns.Length, value => m_canShoot = value));
    }
    public void RotateTurret(Vector3 target)
    {
        m_transform.up = Vector2.Lerp(m_transform.up, (target - m_transform.position).normalized, m_rotateSpeed * Time.deltaTime);
    }
    public void UpdateStats(int penetrationMod, float speedMulti, float damageMulti, float rotateMulti, float fireRateMulti)
    {
        m_penetration = m_basePenetration + penetrationMod;
        if (MaxPenetration = m_penetration > m_maxPenetration) m_penetration = m_maxPenetration;

        m_bulletSpeed = m_baseBulletSpeed * speedMulti;
        if (MaxBulletSpeed = m_bulletSpeed > m_maxBulletSpeed) m_bulletSpeed = m_maxBulletSpeed;

        m_damage = m_baseDamage * damageMulti;
        if (MaxDamage = m_damage > m_maxDamage) m_damage = m_maxDamage;

        m_rotateSpeed = m_baseRotateSpeed * rotateMulti;
        if (MaxRotateSpeed = m_rotateSpeed > m_maxRotateSpeed) m_rotateSpeed = m_maxRotateSpeed;

        m_fireRate = m_baseFireRate * fireRateMulti;
        if (MaxFireRate = m_fireRate > m_maxFireRate) m_fireRate = m_maxFireRate;
    }
}