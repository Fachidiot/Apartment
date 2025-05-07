using UnityEngine;

public enum EState
{
    None,
    Idle,
    Patrol,
    Reaction,
    Run,
    Attack,
    Dead
}

public class EnemyMovement : MonoBehaviour
{
    [SerializeField]
    private EState m_state;

    private Rigidbody m_rigidBody;


    private void Awake()
    {
        m_rigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        m_rigidBody.velocity = new Vector3(-1, m_rigidBody.velocity.y, m_rigidBody.velocity.z);
    }
}
