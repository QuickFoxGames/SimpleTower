using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MGUtilities;
using UnityEngine;
public class BulletPool : Singleton_template<BulletPool>
{
    [SerializeField] private int m_initialCount;
    [SerializeField] private float m_flightTime;
    [SerializeField] private float m_damageFactor;
    [SerializeField] private float m_speedFactor;
    [SerializeField] private Rigidbody2D m_bulletPrefab;
    [SerializeField] private LayerMask m_targetlayers;
    [SerializeField] private Particles2D m_particlesPrefab;
    [SerializeField] private AudioClip m_clip;
    private List<Bullet> m_reserveBullets = new();
    private List<Bullet> m_activeBullets = new();
    private List<Particles2D> m_reserveParticles = new();
    private List<Particles2D> m_activeParticles = new();
    private RaycastHit2D[] m_bulletsHits;
    HashSet<Collider2D> m_processedHits = new();
    private AudioPool m_audioPool;
    void Start()
    {
        m_audioPool = AudioPool.Instance();
        m_bulletsHits = new RaycastHit2D[10];
        for (int i = 0; i < m_initialCount; i++)
        {
            AddBullet();
            AddParticle2D();
        }
    }
    void Update()
    {
        foreach (Bullet b in new List<Bullet>(m_activeBullets))
        {
            b._flightTime += Time.deltaTime;
            if (b._flightTime >= m_flightTime)
            {
                ReturnBullet(b);
                continue;
            }
            Vector2 dir = (Vector2)b._rb.position - b._lastPosition;
            int hitCount = Physics2D.RaycastNonAlloc(b._lastPosition, dir.normalized, m_bulletsHits, dir.magnitude, m_targetlayers);
            if (hitCount > 0)
            {
                m_processedHits.Clear();
                foreach (RaycastHit2D hit in m_bulletsHits)
                {
                    if (hit.collider == null || m_processedHits.Contains(hit.collider)) continue;
                    m_processedHits.Add(hit.collider);

                    var p2d = GetParticle2D(hit.point, Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-359f, 359f)));
                    StartCoroutine(DelayParticleReturn(p2d.AliveTime, p2d));

                    if (b._penetration < 0)
                    {
                        ReturnBullet(b);
                        break;
                    }

                    DamageableObject target = hit.collider.GetComponentInParent<DamageableObject>();
                    if (target != null)
                        target.TakeDamage(b._damage);

                    b._penetration--;
                    b._damage *= m_damageFactor;
                    b._rb.linearVelocity *= m_speedFactor;
                    m_audioPool.SpawnAudioSource(b._rb.position, m_clip, 1f);
                }
                Array.Clear(m_bulletsHits, 0, m_bulletsHits.Length);
            }
            if (b._penetration < 0)
            {
                if (b._isActive) ReturnBullet(b);
                continue;
            }
            b._lastPosition = b._rb.position;
        }
    }
    public Bullet GetBullet(Vector2 pos, Quaternion rot)
    {
        if (m_reserveBullets.Count <= 0) AddBullet();
        Bullet b = m_reserveBullets[^1];
        b._rb.transform.SetPositionAndRotation(pos, rot);
        m_reserveBullets.Remove(b);
        m_activeBullets.Add(b);
        b._rb.gameObject.SetActive(true);
        b._isActive = true;
        return b;
    }
    public void ReturnBullet(Bullet b)
    {
        if (!b._isActive) return;

        b._isActive = false;
        b._rb.gameObject.SetActive(false);
        m_activeBullets.Remove(b);
        m_reserveBullets.Add(b);
        ResetBullet(b);
    }
    private void AddBullet()
    {
        m_reserveBullets.Add(
            new Bullet()
            {
                _isActive = false,
                _penetration = 0,
                _damage = 0f,
                _flightTime = 0f,
                _lastPosition = Vector2.zero,
                _rb = Instantiate(m_bulletPrefab, transform)
            });
        m_reserveBullets[^1]._trailRenderer = m_reserveBullets[^1]._rb.GetComponent<TrailRenderer>();
        m_reserveBullets[^1]._rb.gameObject.SetActive(false);
    }
    private void ResetBullet(Bullet b)
    {
        b._penetration = 0;
        b._damage = 0f;
        b._flightTime = 0f;
        b._lastPosition = Vector2.zero;
        b._rb.linearVelocity = Vector2.zero;
        b._rb.angularVelocity = 0f;
        b._trailRenderer.Clear();
    }
    public Particles2D GetParticle2D(Vector2 pos, Quaternion rot)
    {
        if (m_reserveParticles.Count <= 0) AddParticle2D();
        Particles2D p2d = m_reserveParticles[^1];
        p2d.transform.SetPositionAndRotation(pos, rot);
        m_reserveParticles.Remove(p2d);
        m_activeParticles.Add(p2d);
        p2d.gameObject.SetActive(true);
        p2d.Run();
        return p2d;
    }
    private IEnumerator DelayParticleReturn(float t, Particles2D p2d)
    {
        yield return new WaitForSeconds(t);
        ReturnParticle2D(p2d);
    }
    public void ReturnParticle2D(Particles2D p2d)
    {
        m_reserveParticles.Add(p2d);
        m_activeParticles.Remove(p2d);
        p2d.ResetP2D();
    }
    private void AddParticle2D()
    {
        m_reserveParticles.Add(Instantiate(m_particlesPrefab, transform));
        m_reserveParticles[^1].gameObject.SetActive(false);
    }
}
public class Bullet
{
    public bool _isActive;
    public int _penetration;
    public float _damage;
    public float _flightTime;
    public Vector2 _lastPosition;
    public Rigidbody2D _rb;
    public TrailRenderer _trailRenderer;
}