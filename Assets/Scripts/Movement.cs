using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float m_moveSpeed;
    private Transform m_transform;
    public Vector3 Velocity { get; private set; }
    void Awake()
    {
        m_transform = transform;
    }
    public void Move(Vector3 target)
    {
        Velocity = m_moveSpeed * (target - m_transform.position).normalized;
        m_transform.position += Velocity * Time.deltaTime;
    }
}