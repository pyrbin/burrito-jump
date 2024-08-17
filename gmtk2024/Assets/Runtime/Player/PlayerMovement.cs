using GLTF.Schema.KHR_lights_punctual;
using mote.Runtime.Input;
using pyr.Physics;
using SaintsField;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeReference]
    public MovementSettings Settings;

    [SerializeField]
    private Collider2D _FeetCollider;

    [SerializeField]
    private Collider2D _BodyCollider;

    private Rigidbody2D _Rigidbody;

    [Header("Runtime")]
    [ShowInInspector]
    bool IsGrounded => _IsGrounded;

    [ShowInInspector]
    bool IsFacingRight => _IsFacingRight;

    [ShowInInspector]
    float HorizontalVelocity => _HorizontalVelocity;

    [ShowInInspector]
    float VerticalVelocity => _VerticalVelocity;

    // movement
    private float _HorizontalVelocity;
    private bool _IsFacingRight;

    // collision
    private RaycastHit2D _GroundHit;
    private RaycastHit2D _HeadHit;
    private bool _IsGrounded;
    private bool _BumpedHead;

    // jump var
    private float _VerticalVelocity;
    private bool _IsJumping;
    private bool _IsFastFalling;
    private bool _IsFalling;
    private float _FastFallTime;
    private float _FastFallReleaseSpeed;
    private int _NumberOfJumpsUsed;

    // apex vars
    private float _ApexPoint;
    private float _TimePastApexThreshold;
    private bool _IsPastApexThreshold;
    private float _JumpBufferTimer;
    private bool _JumpReleaseDuringBuffer;

    // coyote tiem vars
    private float _CoyoteTimer;

    // Input
    private GameInput _GameInput;

    private float2 _Input;

    void OnEnable()
    {
        _GameInput ??= new GameInput();
        _GameInput.Enable();
        _GameInput.Gameplay.Enable();
    }

    void Awake()
    {
        _IsFacingRight = true;
        _Rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CountTimers();
        JumpCheck();

        _Input = _GameInput.Gameplay.Move.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if (_IsGrounded)
        {
            Move(Settings.GroundAcceleration, Settings.GroundDeceleration, _Input);
        }
        else
        {
            Move(Settings.AirAceleration, Settings.AirDecceleration, _Input);
        }
    }

    #region Jump

    private void JumpCheck()
    {
        var wasJumpPressed = _GameInput.Gameplay.Jump.WasPressedThisFrame();
        var wasJumpReleased = _GameInput.Gameplay.Jump.WasReleasedThisFrame();

        if (wasJumpPressed)
        {
            _JumpBufferTimer = Settings.JumpBufferTime;
            _JumpReleaseDuringBuffer = false;
        }

        if (wasJumpReleased)
        {
            if (_JumpBufferTimer > 0f)
            {
                _JumpReleaseDuringBuffer = true;
            }

            if (_IsJumping && VerticalVelocity > 0f)
            {
                if (_IsPastApexThreshold)
                {
                    _IsPastApexThreshold = false;
                    _IsFastFalling = true;
                    _FastFallTime = Settings.TimeForUpwardsCancel;
                    _VerticalVelocity = 0f;
                }
                else
                {
                    _IsFastFalling = true;
                    _FastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        if (_JumpBufferTimer > 0f && !_IsJumping && (_IsGrounded || _CoyoteTimer > 0f))
        {
            InitiateJump(1);

            if (_JumpReleaseDuringBuffer)
            {
                _IsFastFalling = true;
                _FastFallReleaseSpeed = VerticalVelocity;
            }
        }
        else if (
            _JumpBufferTimer > 0f
            && _IsJumping
            && _NumberOfJumpsUsed < Settings.NumberOfJumpsAllowed
        )
        {
            // Double Jump
            _IsFastFalling = false;
            InitiateJump(1);
        }
        else if (
            _JumpBufferTimer > 0f
            && _IsFalling
            && _NumberOfJumpsUsed < Settings.NumberOfJumpsAllowed - 1
        )
        {
            InitiateJump(2);
            _IsFastFalling = false;
        }

        if ((_IsJumping || _IsFalling) && _IsGrounded && VerticalVelocity <= 0f)
        {
            _IsJumping = false;
            _IsFalling = false;
            _IsFastFalling = false;
            _FastFallTime = 0;
            _IsPastApexThreshold = false;
            _NumberOfJumpsUsed = 0;

            _VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int numberOfJumpsAllowed)
    {
        if (!_IsJumping)
        {
            _IsJumping = true;
        }

        _JumpBufferTimer = 0f;
        _NumberOfJumpsUsed += numberOfJumpsAllowed;
        _VerticalVelocity = Settings.InitialJumpVelocity;
    }

    private void Jump()
    {
        if (_IsJumping)
        {
            if (_BumpedHead)
            {
                _IsFastFalling = true;
            }

            if (VerticalVelocity >= 0f)
            {
                _ApexPoint = Mathfs.InverseLerp(Settings.InitialJumpVelocity, 0f, VerticalVelocity);
                if (_ApexPoint > Settings.ApexThreshold)
                {
                    if (!_IsPastApexThreshold)
                    {
                        _IsPastApexThreshold = true;
                        _TimePastApexThreshold = 0f;
                    }

                    if (_IsPastApexThreshold)
                    {
                        _TimePastApexThreshold += Time.fixedDeltaTime;
                        if (_TimePastApexThreshold < Settings.ApexHangTime)
                        {
                            _VerticalVelocity = 0f;
                        }
                        else
                        {
                            const f32 k_Nudge = -0.01f;
                            _VerticalVelocity = k_Nudge;
                        }
                    }
                }
                else
                {
                    _VerticalVelocity += Settings.Gravity * Time.fixedDeltaTime;
                    if (_IsPastApexThreshold)
                    {
                        _IsPastApexThreshold = false;
                    }
                }
            }
            else if (!_IsFastFalling)
            {
                _VerticalVelocity +=
                    Settings.Gravity * Settings.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (VerticalVelocity < 0f)
            {
                if (!_IsFalling)
                {
                    _IsFalling = true;
                }
            }
        }

        if (_IsFastFalling)
        {
            if (_FastFallTime >= Settings.TimeForUpwardsCancel)
            {
                _VerticalVelocity +=
                    Settings.Gravity * Settings.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_FastFallTime < Settings.TimeForUpwardsCancel)
            {
                _VerticalVelocity = Mathfs.Lerp(
                    _FastFallReleaseSpeed,
                    0f,
                    _FastFallTime / Settings.TimeForUpwardsCancel
                );
            }
            _FastFallTime += Time.fixedDeltaTime;
        }

        if (!_IsGrounded && !_IsJumping)
        {
            if (!_IsFalling)
            {
                _IsFalling = true;
            }

            _VerticalVelocity += Settings.Gravity * Time.fixedDeltaTime;
        }

        _VerticalVelocity = Mathfs.Clamp(VerticalVelocity, -Settings.MaxFallSpeed, 50f);

        _Rigidbody.linearVelocityY = VerticalVelocity;
    }

    #endregion

    #region Movement

    private void Move(float acc, float decel, float2 moveInput)
    {
        if (math.lengthsq(moveInput) > 0f)
        {
            TurnCheck(moveInput);

            var isRunning = _GameInput.Gameplay.Run.IsPressed();
            var velocity = moveInput.x * (isRunning ? Settings.MaxRunSpeed : Settings.MaxWalkSpeed);

            _HorizontalVelocity = math.lerp(
                _HorizontalVelocity,
                velocity,
                acc * Time.fixedDeltaTime
            );
            _Rigidbody.linearVelocityX = _HorizontalVelocity;
        }
        else
        {
            _HorizontalVelocity = math.lerp(_HorizontalVelocity, 0f, decel * Time.fixedDeltaTime);
            _Rigidbody.linearVelocityX = _HorizontalVelocity;
        }
    }

    private void TurnCheck(float2 moveInput)
    {
        if (_IsFacingRight && moveInput.x < 0f)
        {
            Turn(false);
        }
        else if (!_IsFacingRight && moveInput.x > 0f)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _IsFacingRight = true;
            transform.DORotate(new Vector3(0f, 0f, 0f), 0.33f);
        }
        else
        {
            _IsFacingRight = false;
            transform.DORotate(new Vector3(0f, 180f, 0f), 0.33f);
        }
    }

    #endregion

    #region Collision Checks

    private void CheckIsGrounded()
    {
        var origin = new float2(_FeetCollider.bounds.center.x, _FeetCollider.bounds.min.y);
        var size = new float2(_FeetCollider.bounds.size.x, Settings.GroundDetectionRayLength);

        _GroundHit = Physics2D.BoxCast(
            origin,
            size,
            0f,
            Vector2.down,
            Settings.GroundDetectionRayLength,
            Settings.GroundLayer
        );

        _IsGrounded = _GroundHit.collider != null;
    }

    private void CheckBumpedHead()
    {
        var origin = new float2(_FeetCollider.bounds.center.x, _FeetCollider.bounds.max.y);
        var size = new float2(
            _FeetCollider.bounds.size.x * Settings.HeadWidth,
            Settings.HeadDetectionRayLength
        );

        _HeadHit = Physics2D.BoxCast(
            origin,
            size,
            0f,
            Vector2.up,
            Settings.HeadDetectionRayLength,
            Settings.GroundLayer
        );

        _BumpedHead = _HeadHit.collider != null;
    }

    private void CollisionChecks()
    {
        CheckIsGrounded();
        CheckBumpedHead();
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        _JumpBufferTimer -= Time.deltaTime;
        if (!_IsGrounded)
        {
            _CoyoteTimer -= Time.deltaTime;
        }
        else
        {
            _CoyoteTimer = Settings.JumpCoyoteTime;
        }
    }

    #endregion
}
