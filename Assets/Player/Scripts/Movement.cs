using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float m_walkSpeed = 5f;         // 캐릭터의 걷기 속도
    [SerializeField] private float m_sprintSpeed = 5f;       // 캐릭터의 뛰기 속도
    [SerializeField] private float m_jumpForce = 10f;        // 점프 힘
    [SerializeField] private float m_groundDist = 1f;
    [SerializeField] private LayerMask m_groundLayer;        // 땅으로 인식할 레이어
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    [SerializeField] private bool m_isGrounded;              // 땅에 닿아있는지 여부
    private Rigidbody m_rigidBody;          // 물리 컴포넌트
    private PlayerInputs m_inputs;
    private GameObject m_interactTarget;

    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody>();
        m_inputs = GameManager.Instance.GetComponent<PlayerInputs>();
    }

    void FixedUpdate()
    {
        // 이동 처리
        Move();
        Jump();
    }

    void Update()
    {
        // 인터렉트 처리
        Interaction();
    }

    void LateUpdate()
    {
        // 캐릭터가 땅에 있는지 확인 (원형 충돌 감지)
        Debug.DrawRay(transform.position, Vector3.down, new Color(0,1,0));
        m_isGrounded = Physics.Raycast(transform.position - Vector3.down, Vector3.down, m_groundDist, m_groundLayer);
    }

    private void Move()
    {
        float moveSpeed = m_inputs.GetSprint() ? m_sprintSpeed : m_inputs.GetCrouch() ? m_walkSpeed * 0.5f : m_walkSpeed;

        float horizontal = m_inputs.GetMoveLeft() ? -1 : m_inputs.GetMoveRight() ? 1 : 0;
        float vertical =  m_inputs.GetMoveDown() ? -1 : m_inputs.GetMoveUp() ? 1 : 0;

        if (horizontal != 0)
            m_spriteRenderer.flipX = horizontal > 0;

        if (m_isGrounded)
            m_rigidBody.velocity = new Vector3(horizontal * moveSpeed, m_rigidBody.velocity.y, vertical * moveSpeed);
    }

    private void Jump()
    {
        // 점프 입력 처리 (스페이스바를 누르고 땅에 있을 때만)
        if (m_inputs.GetJump() && m_isGrounded)
        {
            m_rigidBody.velocity = new Vector2(m_rigidBody.velocity.x, m_jumpForce);
        }
    }

    private void Interaction()
    {
        if (m_inputs.GetInteract())
        {
            if (m_interactTarget != null)
                m_interactTarget.GetComponent<InteractableDoor>().InteractDoor();
        }
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
