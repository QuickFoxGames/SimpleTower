using UnityEngine;
using UnityEngine.UI;
public class DamageableObject : MonoBehaviour
{
    [SerializeField] private float m_maxHp;
    [SerializeField] private float m_regenRate;
    [SerializeField] protected Slider m_hpBar;
    public bool IsDead { get; private set; }
    public float CurrentHp { get; private set; }
    private void Awake()
    {
        UpdateHP(m_maxHp);
    }
    public void RunRegen()
    {
        if (CurrentHp < m_maxHp)
        {
            CurrentHp += m_regenRate * Time.deltaTime;
            if (m_hpBar) m_hpBar.value = CurrentHp;
        }
    }
    public void TakeDamage(float d)
    {
        CurrentHp -= d;
        IsDead = CurrentHp <= 0;
        if (m_hpBar) m_hpBar.value = CurrentHp;
    }
    public void ResetDO()
    {
        IsDead = false;
        CurrentHp = m_maxHp;
    }
    public void UpdateHP(float hp)
    {
        m_maxHp = hp;
        CurrentHp = m_maxHp;
        if (m_hpBar)
        {
            m_hpBar.maxValue = m_maxHp;
            m_hpBar.value = CurrentHp;
        }
    }
}