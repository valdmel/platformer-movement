using UnityEngine;

public abstract class InputReader : MonoBehaviour
{
    #region VARIABLES
    #region SERIALIZABLE
    #endregion
    
    protected GameInput gameInput;
    #endregion
    
    #region MONOBEHAVIOUR METHODS
    protected void OnEnable()
    {
        gameInput = new GameInput();

        gameInput.Enable();
    }
    
    protected void OnDisable() => gameInput.Disable();
    #endregion
}