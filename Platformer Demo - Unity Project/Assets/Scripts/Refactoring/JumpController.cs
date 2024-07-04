using UnityEngine;

public class JumpController : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GroundDetector groundDetector;
    
    private Rigidbody2D rigidbody;
    private DashController dashController;
    private bool isFacingRight;
    
    public bool IsJumping { get; set; }
    public bool IsJumpCut { get; set; }
    public bool IsJumpFalling { get; set; }
    private float LastPressedJumpTime { get; set; }
    
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        dashController = GetComponent<DashController>();
    }

    private void Start()
    {
        SetGravityScale(playerData.gravityScale);
		
        isFacingRight = true;
    }
    
    private void Update()
    {
        UpdateTimers();
        HandleJump();
        CheckCollisions();
    }

    private void FixedUpdate() => HandleGravity();

    private void UpdateTimers() => LastPressedJumpTime -= Time.deltaTime;

    private void HandleJump()
    {
        if (IsJumping && rigidbody.velocity.y < 0f)
        {
            IsJumping = false;
            IsJumpFalling = true;
        }

        if (groundDetector.LastOnGroundTime > 0f && !IsJumping)
        {
            IsJumpCut = false;

            if (!IsJumping) IsJumpFalling = false;
        }

        if (dashController.IsDashing) return;
        
        if (CanJump() && LastPressedJumpTime > 0f)
        {
            IsJumping = true;
            IsJumpCut = false;
            IsJumpFalling = false;

            Jump();
        }
    }

    private void HandleGravity()
    {
        if (!dashController.IsDashing)
        {
            //Higher gravity if we've released the jump input or are falling
            if (rigidbody.velocity.y < 0f)
            {
                //Much higher gravity if holding down
                SetGravityScale(playerData.gravityScale * playerData.fastFallGravityMult);
                //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
                rigidbody.velocity = new Vector2(rigidbody.velocity.x,
                    Mathf.Max(rigidbody.velocity.y, -playerData.maxFastFallSpeed));
            }
            else if (IsJumpCut)
            {
                //Higher gravity if jump button released
                SetGravityScale(playerData.gravityScale * playerData.jumpCutGravityMult);
                
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, Mathf.Max(rigidbody.velocity.y, -playerData.maxFallSpeed));
            }
            else if ((IsJumping || IsJumpFalling) && Mathf.Abs(rigidbody.velocity.y) < playerData.jumpHangTimeThreshold)
            {
                SetGravityScale(playerData.gravityScale * playerData.jumpHangGravityMult);
            }
            else if (rigidbody.velocity.y < 0f)
            {
                //Higher gravity if falling
                SetGravityScale(playerData.gravityScale * playerData.fallGravityMult);
                //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, Mathf.Max(rigidbody.velocity.y, -playerData.maxFallSpeed));
            }
            else
            {
                //Default gravity if standing on a platform or moving upwards
                SetGravityScale(playerData.gravityScale);
            }
        }
        else
        {
            //No gravity when dashing (returns to normal once initial dashAttack phase over)
            SetGravityScale(0f);
        }
    }
    
    private void CheckCollisions()
    {
        if (dashController.IsDashing || IsJumping) return;
        
        if (groundDetector.Detect() && !IsJumping) groundDetector.LastOnGroundTime = playerData.coyoteTime;
    }
    
    private void SetGravityScale(float scale) => rigidbody.gravityScale = scale;
    
    public void OnJumpInput() => LastPressedJumpTime = playerData.jumpInputBufferTime;

    public void OnJumpUpInput()
    {
        if (CanJumpCut()) IsJumpCut = true;
    }
    
    private void Jump()
    {
        LastPressedJumpTime = 0f;
        groundDetector.LastOnGroundTime = 0f;
        
        var jumpForce = playerData.jumpForce * Vector2.up;
		
        if (rigidbody.velocity.y < 0f) jumpForce.y -= rigidbody.velocity.y;

        rigidbody.AddForce(jumpForce, ForceMode2D.Impulse);
    }
    
    private bool CanJump() => groundDetector.LastOnGroundTime > 0f && !IsJumping;

    private bool CanJumpCut() => IsJumping && rigidbody.velocity.y > 0f;
}