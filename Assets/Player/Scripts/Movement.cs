using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    private float m_walkSpeed = 5f;         // ĳ������ �̵� �ӵ�
    [SerializeField]
    private float m_sprintSpeed = 5f;       // ĳ������ �̵� �ӵ�
    [SerializeField]
    private float m_jumpForce = 10f;        // ���� ��
    [SerializeField]
    private float m_groundDist = 1f;
    [SerializeField]
    private LayerMask m_groundLayer;        // ������ �ν��� ���̾�
    [SerializeField]
    private SpriteRenderer m_spriteRenderer;
    [SerializeField]
    private bool m_isGrounded;              // ���� ����ִ��� ����

    private CapsuleCollider m_capsuleCollider;
    private Rigidbody m_rigidBody;          // ���� ������Ʈ
    private Inputs m_inputs;

    private GameObject m_interactTarget;

    void Start()
    {
        m_capsuleCollider = GetComponent<CapsuleCollider>();
        m_rigidBody = GetComponent<Rigidbody>();
        m_inputs = GetComponent<Inputs>();
    }

    void Update()
    {
        // �̵� ó��
        Move();
        Jump();
        // ���ͷ�Ʈ ó��
        Interaction();
    }

    private void Move()
    {
        float moveSpeed = m_inputs.sprint ? m_sprintSpeed : m_inputs.crouch ? m_walkSpeed * 0.5f : m_walkSpeed;

        float horizontal = m_inputs.move.x;
        float vertical = m_inputs.move.y;

        if (horizontal != 0)
            m_spriteRenderer.flipX = horizontal > 0;

        if (m_isGrounded)
            m_rigidBody.velocity = new Vector3(horizontal * moveSpeed, m_rigidBody.velocity.y, vertical * moveSpeed);
    }

    private void Jump()
    {
        // ���� �Է� ó�� (�����̽��ٸ� ������ ���� ���� ����)
        if (Input.GetButtonDown("Jump") && m_isGrounded)
        {
            m_rigidBody.velocity = new Vector2(m_rigidBody.velocity.x, m_jumpForce);
        }
    }

    private void Interaction()
    {
        if (m_inputs.interact)
        {
            if (m_interactTarget != null)
                m_interactTarget.GetComponent<InteractableDoor>().InteractDoor();

            m_inputs.interact = false;
        }
    }

    void FixedUpdate()
    {
        Debug.DrawRay(transform.position, Vector3.down, new Color(0,1,0));
        // ĳ���Ͱ� ���� �ִ��� Ȯ�� (���� �浹 ����)
        m_isGrounded = Physics.Raycast(transform.position - Vector3.down, Vector3.down, m_groundDist, m_groundLayer);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Door")
            m_interactTarget = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Door")
            m_interactTarget = null;
    }
}
