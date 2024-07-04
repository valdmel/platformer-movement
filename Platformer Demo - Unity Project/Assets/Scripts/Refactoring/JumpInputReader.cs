using UnityEngine.InputSystem;

public class JumpInputReader : InputReader
{
    private JumpController jumpController;
    
    #region MONOBEHAVIOUR METHODS
    protected void OnEnable()
    {
        base.OnEnable();
        gameInput.Player.Jump.performed += OnJump;
        gameInput.Player.Jump.canceled += OnJumpCanceled;
    }

    private void Awake() => jumpController = GetComponent<JumpController>();

    protected void OnDisable()
    {
        base.OnDisable();
        gameInput.Player.Jump.performed -= OnJump;
        gameInput.Player.Jump.canceled -= OnJumpCanceled;
    }
    #endregion
    
    private void OnJump(InputAction.CallbackContext inputContext) => jumpController.OnJumpInput();

    private void OnJumpCanceled(InputAction.CallbackContext inputContext) => jumpController.OnJumpUpInput();
}