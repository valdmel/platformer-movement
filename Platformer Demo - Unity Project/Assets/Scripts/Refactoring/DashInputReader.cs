using UnityEngine;
using UnityEngine.InputSystem;

public class DashInputReader : InputReader
{
    private DashController dashController;

    #region MONOBEHAVIOUR METHODS
    protected void OnEnable()
    {
        base.OnEnable();
        gameInput.Player.Dash.performed += OnDash;
    }

    private void Awake() => dashController = GetComponent<DashController>();

    protected void OnDisable()
    {
        base.OnDisable();
        gameInput.Player.Dash.performed -= OnDash;
    }
    #endregion

    private void OnDash(InputAction.CallbackContext inputContext)
    {
        var dashDirection = Mathf.Approximately(transform.localRotation.eulerAngles.y, 0f) ? Vector2.right : Vector2.left;

        dashController.OnDashInput();
        dashController.MoveTo(dashDirection);
    }
}