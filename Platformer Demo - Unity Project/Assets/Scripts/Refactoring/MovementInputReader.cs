using UnityEngine;
using UnityEngine.InputSystem;

public class MovementInputReader : InputReader
{
    private MovementController movementController;

    #region MONOBEHAVIOUR METHODS
    protected void OnEnable()
    {
        base.OnEnable();
        gameInput.Player.Move.performed += OnMove;
        gameInput.Player.Move.canceled += OnMove;
    }

    private void Awake() => movementController = GetComponent<MovementController>();

    protected void OnDisable()
    {
        base.OnDisable();
        gameInput.Player.Move.performed -= OnMove;
        gameInput.Player.Move.canceled -= OnMove;
    }
    #endregion

    private void OnMove(InputAction.CallbackContext inputContext)
    {
        var movementDirection = inputContext.ReadValue<Vector2>();
        
        movementController.MoveTo(movementDirection);
    }
}