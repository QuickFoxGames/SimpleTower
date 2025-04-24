using System.Collections;
using System.Collections.Generic;
using MGUtilities;
using UnityEngine;
public class Particles2D : MonoBehaviour
{
    [SerializeField] private bool m_playOnAwake;
    [SerializeField] private int m_particleCount;
    [SerializeField] private Vector2 m_speed;
    [SerializeField] private float m_lifeTime;
    [SerializeField] private Rigidbody2D m_particlePrefab;
    private Coroutine m_coroutine = null;
    private List<Rigidbody2D> m_reserveParticles = new();
    private List<Rigidbody2D> m_activeParticles = new();
    public float AliveTime { get { return m_lifeTime; } }
    private void Awake()
    {
        if (m_playOnAwake) Run();
        for (int i = 0; i < m_particleCount; i++)
        {
            AddParticle();
        }
    }
    public void ResetP2D()
    {
        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
            m_coroutine = null;
        }

        foreach (var p in new List<Rigidbody2D>(m_activeParticles))
        {
            ReturnParticle2D(p);
        }

        transform.position = Vector3.zero;
        gameObject.SetActive(false);
    }
    public void Run()
    {
        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
            m_coroutine = null;
        }
        m_coroutine = StartCoroutine(RunParticleEffect());
    }
    private IEnumerator RunParticleEffect()
    {
        for (int i = 0; i < m_particleCount; i++)
        {
            Rigidbody2D p = GetParticle(Vector2.zero, Quaternion.identity);
            Vector2 dir = Random.insideUnitCircle.normalized;
            p.linearVelocity = dir * Random.Range(m_speed.x, m_speed.y);
            StartCoroutine(Coroutines.LerpVector3OverTime(p.transform.localScale, Vector3.zero, m_lifeTime * 0.9f, value => p.transform.localScale = value));
        }
        yield return new WaitForSeconds(m_lifeTime * 0.97f);
        foreach (var p in new List<Rigidbody2D>(m_activeParticles))
        {
            ReturnParticle2D(p);
        }
    }
    public Rigidbody2D GetParticle(Vector2 pos, Quaternion rot)
    {
        if (m_reserveParticles.Count <= 0) AddParticle();
        Rigidbody2D p = m_reserveParticles[^1];
        m_reserveParticles.Remove(p);
        m_activeParticles.Add(p);
        p.transform.SetLocalPositionAndRotation(pos, rot);
        p.gameObject.SetActive(true);
        return p;
    }
    public void ReturnParticle2D(Rigidbody2D p)
    {
        m_activeParticles.Remove(p);
        m_reserveParticles.Add(p);
        p.linearVelocity = Vector2.zero;
        p.angularVelocity = 0f;
        p.transform.localPosition = Vector3.zero;
        p.transform.localScale = m_particlePrefab.transform.localScale;
        p.gameObject.SetActive(false);
    }
    private void AddParticle()
    {
        m_reserveParticles.Add(Instantiate(m_particlePrefab, transform));
        m_reserveParticles[^1].gameObject.SetActive(false);
    }
}