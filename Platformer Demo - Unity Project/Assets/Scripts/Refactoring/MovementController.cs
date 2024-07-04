using UnityEngine;

public class MovementController : MonoBehaviour
{
    private const float RotationAngle = 180f;
    
    [SerializeField] private PlayerData playerData;

    private Rigidbody2D rigidbody;
    private bool IsFacingRight;
    private float LastOnGroundTime;
    private Vector2 movementDirection;
    private bool isFacingRight;
    
    private void Awake() => rigidbody = GetComponent<Rigidbody2D>();

    private void Start() => InitializeFacingDirection();

    private void FixedUpdate() => HandleMovement();

    private void HandleMovement()
    {
        var targetSpeed = movementDirection.x * playerData.runMaxSpeed;
        targetSpeed = Mathf.Lerp(rigidbody.velocity.x, targetSpeed, 1f);
        var accelerationRate = GetAccelerationRate(targetSpeed);

        ConserveMomentum(ref accelerationRate, ref targetSpeed);

        var speedDelta = targetSpeed - rigidbody.velocity.x;
        var movementSpeed = speedDelta * accelerationRate;
        var movementForce = movementSpeed * Vector2.right;

        rigidbody.AddForce(movementForce, ForceMode2D.Force);
        CheckDirectionToFace(movementDirection.x > 0f);
    }

    public void MoveTo(Vector2 direction) => movementDirection = direction;

    private float GetAccelerationRate(float targetSpeed)
    {
        if (LastOnGroundTime > 0f) return Mathf.Abs(targetSpeed) > 0.01f ? playerData.runAccelAmount : playerData.runDeccelAmount;

        return Mathf.Abs(targetSpeed) > 0.01f
            ? playerData.runAccelAmount * playerData.accelInAir
            : playerData.runDeccelAmount * playerData.deccelInAir;
    }

    private void ConserveMomentum(ref float accelerationRate, ref float targetSpeed)
    {
        if (playerData.doConserveMomentum && Mathf.Abs(rigidbody.velocity.x) > Mathf.Abs(targetSpeed) &&
            Mathf.Approximately(Mathf.Sign(rigidbody.velocity.x), Mathf.Sign(targetSpeed)) &&
            Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0f)
        {
            accelerationRate = 0f;
        }
    }

    private void CheckDirectionToFace(bool isMovingRight)
    {
        var isIdle = Mathf.Approximately(movementDirection.x, 0f);

        if (isIdle) return;

        if (isMovingRight == isFacingRight) return;

        isFacingRight = isMovingRight;

        Rotate(RotationAngle);
    }

    private void InitializeFacingDirection() => isFacingRight = IsCurrentlyFacingRight();

    private bool IsCurrentlyFacingRight() => Mathf.Approximately(transform.localRotation.eulerAngles.y, 0f);

    private void Rotate(float angle) => transform.Rotate(Vector3.up, angle);
}