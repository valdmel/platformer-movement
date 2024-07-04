using System.Collections;
using UnityEngine;

public class DashController : MonoBehaviour
{
	public bool IsDashing { get; set; }
	public PlayerData PlayerData
	{
		get => playerData;
		set => playerData = value;
	}
	private float LastPressedDashTime { get; set; }
	
	private int dashesLeft;
	private bool dashRefilling;
	private Vector2 dashDirection;
	private Rigidbody2D rigidbody;
	private JumpController jumpController;
	
	[SerializeField] private PlayerData playerData;
	[SerializeField] private GroundDetector groundDetector;
	
	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody2D>();
		jumpController = GetComponent<JumpController>();
	}
	
	private void Update()
	{
		HandleTimers();
		HandleDash();
		CheckCollisions();
	}
	
	private void FixedUpdate()
	{
		if (IsDashing) Dash();
		
		HandleGravity();
	}

	private void HandleDash()
	{
		if (CanDash() && LastPressedDashTime > 0f)
		{
			//Freeze game for split second. Adds juiciness and a bit of forgiveness over directional input
			StartCoroutine(Sleep(playerData.dashSleepTime));

			IsDashing = true;
			jumpController.IsJumping = false;
			jumpController.IsJumpCut = false;

			StartCoroutine(StartDash());
		}
	}
	
	private void HandleTimers() => LastPressedDashTime -= Time.deltaTime;

	private void HandleGravity()
	{
		if (IsDashing) SetGravityScale(0f);
		/*else
		{
			//Higher gravity if we've released the jump input or are falling
			if (rigidbody.velocity.y < 0f && moveInput.y < 0f)
			{
				Debug.Log("1");
				//Much higher gravity if holding down
				SetGravityScale(playerData.gravityScale * playerData.fastFallGravityMult);
				//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
				rigidbody.velocity = new Vector2(rigidbody.velocity.x,
					Mathf.Max(rigidbody.velocity.y, -playerData.maxFastFallSpeed));
			}
			else if (isJumpCut)
			{
				Debug.Log("2");
				//Higher gravity if jump button released
				SetGravityScale(playerData.gravityScale * playerData.jumpCutGravityMult);
				rigidbody.velocity = new Vector2(rigidbody.velocity.x,
					Mathf.Max(rigidbody.velocity.y, -playerData.maxFallSpeed));
			}
			else if ((IsJumping || isJumpFalling) && Mathf.Abs(rigidbody.velocity.y) < playerData.jumpHangTimeThreshold)
			{
				SetGravityScale(playerData.gravityScale * playerData.jumpHangGravityMult);
			}
			else if (rigidbody.velocity.y < 0f)
			{
				//Higher gravity if falling
				SetGravityScale(playerData.gravityScale * playerData.fallGravityMult);
				//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
				rigidbody.velocity = new Vector2(rigidbody.velocity.x,
					Mathf.Max(rigidbody.velocity.y, -playerData.maxFallSpeed));
			}
			else
			{
				//Default gravity if standing on a platform or moving upwards
				SetGravityScale(playerData.gravityScale);
			}
		}*/
	}

	private void CheckCollisions()
	{
		if (!IsDashing && !jumpController.IsJumping)
		{
			if (groundDetector.Detect() && !jumpController.IsJumping) groundDetector.LastOnGroundTime = playerData.coyoteTime;
		}
	}

	public void OnDashInput() => LastPressedDashTime = playerData.dashInputBufferTime;

	private void SetGravityScale(float scale) => rigidbody.gravityScale = scale;

	private IEnumerator Sleep(float duration)
	{
		Time.timeScale = 0f;

		yield return new WaitForSecondsRealtime(duration);

		Time.timeScale = 1f;
	}
	
	public void MoveTo(Vector2 direction)
	{
		dashDirection = direction;
	}

	private void Dash()
	{
		// Calculate the target speed based on input and maximum speed
		var targetSpeed = dashDirection.x * playerData.runMaxSpeed;
    
		// Smoothly adjust current velocity towards target speed
		targetSpeed = Mathf.Lerp(rigidbody.velocity.x, targetSpeed, playerData.dashEndRunLerp);

		// Determine acceleration rate based on ground status and movement conditions
		var accelerationRate = groundDetector.LastOnGroundTime > 0f
			? Mathf.Abs(targetSpeed) > 0.01f ? playerData.runAccelAmount : playerData.runDeccelAmount
			: Mathf.Abs(targetSpeed) > 0.01f ? playerData.runAccelAmount * playerData.accelInAir : playerData.runDeccelAmount * playerData.deccelInAir;

		// Adjust acceleration and max speed if jumping at apex
		if ((jumpController.IsJumping || jumpController.IsJumpFalling) && Mathf.Abs(rigidbody.velocity.y) < playerData.jumpHangTimeThreshold)
		{
			accelerationRate *= playerData.jumpHangAccelerationMult;
			targetSpeed *= playerData.jumpHangMaxSpeedMult;
		}

		// Conserve momentum if enabled and conditions are met
		if (playerData.doConserveMomentum && Mathf.Abs(rigidbody.velocity.x) > Mathf.Abs(targetSpeed) &&
		    Mathf.Approximately(Mathf.Sign(rigidbody.velocity.x), Mathf.Sign(targetSpeed)) &&
		    Mathf.Abs(targetSpeed) > 0.01f && groundDetector.LastOnGroundTime < 0f)
		{
			accelerationRate = 0f; // Prevent deceleration to conserve momentum
		}

		// Calculate velocity difference and movement force
		var speedDifference = targetSpeed - rigidbody.velocity.x;
		var movementForce = speedDifference * accelerationRate;

		// Apply movement force along the x-axis to the rigidbody
		rigidbody.AddForce(movementForce * Vector2.right, ForceMode2D.Force);
	}
	
	private IEnumerator StartDash()
	{
		//Overall this method of dashing aims to mimic Celeste, if you're looking for
		// a more physics-based approach try a method similar to that used in the jump
		groundDetector.LastOnGroundTime = 0f;
		LastPressedDashTime = 0f;
		var startTime = Time.time;
		dashesLeft--;
		IsDashing = true;

		SetGravityScale(0f);

		//We keep the player's velocity at the dash speed during the "attack" phase (in celeste the first 0.15s)
		while (Time.time - startTime <= playerData.dashAttackTime)
		{
			rigidbody.velocity = dashDirection.normalized * playerData.dashSpeed;
			//Pauses the loop until the next frame, creating something of a Update loop. 
			//This is a cleaner implementation opposed to multiple timers and this coroutine approach is actually what is used in Celeste :D
			yield return null;
		}

		startTime = Time.time;
		IsDashing = false;

		//Begins the "end" of our dash where we return some control to the player but still limit run acceleration (see Update() and Run())
		SetGravityScale(playerData.gravityScale);
		
		rigidbody.velocity = playerData.dashEndSpeed * dashDirection.normalized;

		while (Time.time - startTime <= playerData.dashEndTime) yield return null;
		
		IsDashing = false;
	}
	
	private IEnumerator RefillDash(int amount)
	{
		//SHoet cooldown, so we can't constantly dash along the ground, again this is the implementation in Celeste, feel free to change it up
		dashRefilling = true;
		
		yield return new WaitForSeconds(playerData.dashRefillTime);
		
		dashRefilling = false;
		dashesLeft = Mathf.Min(playerData.dashAmount, amount);
	}

	private bool CanDash()
	{
		if (!IsDashing && dashesLeft < playerData.dashAmount && groundDetector.LastOnGroundTime > 0f && !dashRefilling) StartCoroutine(RefillDash(1));

		return dashesLeft > 0f;
	}
}