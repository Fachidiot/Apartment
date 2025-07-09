using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private AttackController _attackController;
    private WeaponController _weaponController;
    private LifeController _lifeController;
    private PlayerInputs _inputs;
    private Rigidbody _rigidbody;

    void Start()
    {
        _attackController = GetComponent<AttackController>();
        _weaponController = GetComponent<WeaponController>();
        _lifeController = GetComponent<LifeController>();
        _rigidbody = GetComponent<Rigidbody>();
        _inputs = GameManager.Instance.GetComponent<PlayerInputs>();
    }

    void Update()
    {
        // Debug.Log(rigidbody.velocity);
        animator.SetFloat("Velocity", _rigidbody.velocity.magnitude);
        animator.SetBool("InputMove", _inputs.GetMoveLeft() || _inputs.GetMoveRight());

        if (_inputs.GetAttack())
        {   // Attack functions
            Weapon weapon = _weaponController.GetCurrentWeapon();    // get weapon
            _attackController.DoAttack(weapon);  // do attack.
        }
    }
}
