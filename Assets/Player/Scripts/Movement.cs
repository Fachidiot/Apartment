using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float m_walkSpeed = 5f;         // 캐릭터의 걷기 속도
    [SerializeField] private float m_sprintSpeed = 5f;       // 캐릭터의 뛰기 속도
    [SerializeField] private float m_jumpForce = 10f;        // 점프 힘
    [SerializeField] private float m_jumpCoolTime = 0.3f;   // 점프 쿨타임
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    [SerializeField] private bool m_isGrounded;              // 땅에 닿아있는지 여부
    private Rigidbody rigidBody;          // 물리 컴포넌트
    private PlayerInputs inputs;
    private GameObject interactTarget;

    private float jumpCoolTime = 0f;
    private GroundCheck groundChecker;

    void Start()
    {
        groundChecker = GetComponent<GroundCheck>();
        rigidBody = GetComponent<Rigidbody>();
        inputs = GameManager.Instance.GetComponent<PlayerInputs>();
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
        // Ground Check
        m_isGrounded = groundChecker.IsGrounded();
    }

    private void Move()
    {
        float moveSpeed = inputs.GetSprint() ? m_sprintSpeed : inputs.GetCrouch() ? m_walkSpeed * 0.5f : m_walkSpeed;

        float horizontal = inputs.GetMoveLeft() ? -1 : inputs.GetMoveRight() ? 1 : 0;
        float vertical = inputs.GetMoveDown() ? -1 : inputs.GetMoveUp() ? 1 : 0;

        if (horizontal != 0 && m_isGrounded)    // 이동 방향에 따라 Sprite Flip
            m_spriteRenderer.flipX = horizontal < 0;

        if (m_isGrounded)   // Ground 상태에서만 좌우 움직임 적용.
            rigidBody.velocity = new Vector3(horizontal * moveSpeed, rigidBody.velocity.y, vertical * moveSpeed);
    }

    private void Jump()
    {
        // 점프 입력 처리 (스페이스바를 누르고 땅에 있을 때만)
        if (inputs.GetJump() && m_isGrounded && jumpCoolTime > m_jumpCoolTime)
        {
            Debug.Log("Jump");
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, m_jumpForce);
        }

        if (!m_isGrounded)
        {   // 공중에 떠있을때.
            jumpCoolTime = 0f;
            rigidBody.useGravity = true;
        }
        else
        {   // 착지시.
            jumpCoolTime += Time.deltaTime;
            rigidBody.useGravity = false;
        }
    }

    private void Interaction()
    {
        if (inputs.GetInteract())
        {
            if (interactTarget != null)
                interactTarget.GetComponent<InteractableDoor>().InteractDoor();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Door")
            interactTarget = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Door")
            interactTarget = null;
    }
}
