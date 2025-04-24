using UnityEngine;
[RequireComponent(typeof(Movement))]
public class Enemy : MonoBehaviour
{
    [SerializeField] protected int m_scoreOnDeath;
    [SerializeField] protected float m_damage;
    protected AudioPool m_audioPool;
    protected DamageableObject m_damageableObject;
    protected Tower m_tower;
    protected Movement m_movement;
    public bool IsDead { get { return m_damageableObject.IsDead; } }
    public int ScoreOnDeath { get { return m_scoreOnDeath; } }
    public Vector3 Velocity { get; private set; }
    protected virtual void Awake()
    {
        m_damageableObject = GetComponent<DamageableObject>();
        m_movement = GetComponent<Movement>();
        m_tower = Tower.Instance();
        m_audioPool = AudioPool.Instance();
    }
    public void RunUpdate()
    {
        if (Vector2.Distance(m_tower.transform.position, transform.position) >= 0.5f) m_movement.Move(m_tower.transform.position);
        else m_tower.DamageTower(m_damage * Time.deltaTime);
    }
    public void ResetEnemy()
    {
        m_damageableObject.ResetDO();
    }
    public void SetHP(float hp)
    {
        m_damageableObject.UpdateHP(hp);
    }
}