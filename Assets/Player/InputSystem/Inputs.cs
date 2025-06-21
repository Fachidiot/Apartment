using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Inputs : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool escape;
    public bool canJump;
    public bool jump;
    public bool sprint;
    public bool slide;
    public bool crouch;
    public bool interact;

    public bool aim;
    public bool click;
    public bool inventory;
    public bool unarmed;
    public bool build;
    public bool reload;
    public bool num1;
    public bool num2;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorInputForLook = true;

    // Buttons
    public void MoveLeftDown()
    {
        MoveInput(Vector2.left);
    }
    public void MoveLeftUp()
    {
        MoveInput(Vector2.zero);
    }

    public void MoveRightDown()
    {
        MoveInput(Vector2.right);
    }
    public void MoveRightUp()
    {
        MoveInput(Vector2.zero);
    }

    public void InteractiveDown()
    {
        InteractInput(true);
    }
    public void InteractiveUp()
    {
        InteractInput(false);
    }

#if ENABLE_INPUT_SYSTEM
    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    public void OnEscape(InputValue value)
    {
        EscapeInput(value.isPressed);
    }

    public void OnJump(InputValue value)
    {
        JumpInput(value.isPressed);
    }

    public void OnSprint(InputValue value)
    {
        SprintInput(value.isPressed);
    }

    public void OnCrouch(InputValue value)
    {
        CrouchInput();
    }

    public void OnInteract(InputValue value)
    {
        InteractInput(value.isPressed);
    }

#endif
    private void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    private void EscapeInput(bool newEscapeState)
    {
        escape = newEscapeState;
    }

    private void JumpInput(bool newJumpState)
    {
        if (canJump)
            jump = newJumpState;
    }

    private void SprintInput(bool newSprintState)
    {
        if (aim)
            return;
        sprint = newSprintState;
        crouch = false;
    }

    private void CrouchInput()
    {
        if (sprint)
        {
            slide = true;
            return;
        }
        crouch = !crouch;
    }

    private void InteractInput(bool newInteractState)
    {
        interact = newInteractState;
    }

    [Header("UI Settings")]
    public bool isFocus = true;

    private void OnApplicationFocus(bool hasFocus)
    {
        //SetCursorState(cursorLocked);
    }

    public void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
