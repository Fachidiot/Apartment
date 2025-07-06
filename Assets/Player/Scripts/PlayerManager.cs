using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private LifeController lifeController;
    private PlayerInputs inputs;
    private Rigidbody rigidbody;

    void Start()
    {
        lifeController = GetComponent<LifeController>();
        rigidbody = GetComponent<Rigidbody>();
        inputs = GameManager.Instance.GetComponent<PlayerInputs>();
    }

    void Update()
    {
        // Debug.Log(rigidbody.velocity);
        animator.SetFloat("Velocity", rigidbody.velocity.magnitude);
        animator.SetBool("InputMove", inputs.GetMoveLeft() || inputs.GetMoveRight());
    }
}
