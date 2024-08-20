using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    [Serializable]
    public enum MovementState
    {
        Frozen,
        Free
    }

    [Header("References")]
    [SerializeField]
    private Collider2D _Feet;

    [SerializeField]
    private Collider2D _Body;

    [Header("Movement")]
    [field: SerializeField]
    public float JumpHeight { get; private set; } = 2f;
    public float Acceleration = 15f;

    [field: SerializeField]
    public float GravityFactor { get; private set; } = 1f;

    [field: SerializeField]
    public float MaxSpeed { get; private set; } = 10f;

    [field: SerializeField]
    public float MaxVertical { get; private set; } = 20f;

    [field: SerializeField]
    public float MaxVerticalUp { get; private set; } = 12f;

    [Min(0)]
    [SerializeField]
    private float _SurfaceAnchor = 0.05f;

    [Range(0, 90)]
    [SerializeField]
    private float _MaxSlop = 45f;

    public float AirAccelerationFactor = 0.75f;
    public Duration JumpGracePeriod = 0.33f.Secs();
    public LayerMask GroundLayer;

    [SerializeField]
    [ShowInInspector]
    [ReadOnly]
    private float2 _Velocity;

    [SerializeField]
    [ShowInInspector]
    [ReadOnly]
    private Rigidbody2D.SlideMovement _SlideMovement = new Rigidbody2D.SlideMovement();

    [ReadOnly]
    public bool _IsGrounded = true;
    private RaycastHit2D _GroundHit;

    [Header("Visuals")]
    public Transform ModelPivot;
    public Animator Animator;
    public Transform PuffParticleSpawnPoint;
    public GameObject PuffParticlePrefab;

    // Private
    private float _MinGroundVertical;
    private float _JumpForce;

    public Vector2 Velocity
    {
        get => _Velocity;
        set
        {
            _Velocity = new Vector2(
                math.clamp(value.x, -MaxSpeed, MaxSpeed),
                math.clamp(value.y, -MaxVertical, MaxVerticalUp)
            );
        }
    }

    public float Direction
    {
        get => _Direction;
        set
        {
            _Direction = value;
            Velocity = new float2(
                value * (Acceleration * (!IsGrounded ? AirAccelerationFactor : 1f)),
                _Velocity.y
            );
        }
    }
    private float _Direction;

    public bool IsFacingRight { get; private set; } = true;

    private Rigidbody2D _Rigidbody;

    [SerializeField]
    public MovementState State = MovementState.Free;

    public bool IsMovingSideways => math.abs(Velocity.x) > 0;

    public bool IsAcceleratingSideways => Direction != 0;

    public bool IsGrounded => _IsGrounded;

    public bool IsFalling => Velocity.y < 0 && !IsGrounded;

    private Timer _JumpGracePeriodTimer;

    // events.
    public event Action? OnJump;
    public event Action<float>? OnFell;

    void Awake()
    {
        IsFacingRight = true;
        _JumpGracePeriodTimer = new Timer(JumpGracePeriod);
        _JumpGracePeriodTimer.Tick(JumpGracePeriod);
        _Rigidbody = GetComponent<Rigidbody2D>();

        _MinGroundVertical = Mathf.Cos(_MaxSlop * Mathf.PI / 180f);
        _SlideMovement = CreateSlideMovement();
        _JumpForce = math.sqrt(2 * Physics2D.gravity.magnitude * GravityFactor * JumpHeight);
        Animator.SetBool("falling", false);
        Animator.SetBool("walk", false);
        Animator.SetBool("idle", true);
    }

    private Rigidbody2D.SlideMovement CreateSlideMovement()
    {
        return new Rigidbody2D.SlideMovement
        {
            maxIterations = 3,
            surfaceSlideAngle = 90,
            gravitySlipAngle = 90,
            surfaceUp = Vector2.up,
            surfaceAnchor = Vector2.down * _SurfaceAnchor,
            gravity = Vector2.zero,
            layerMask = GroundLayer,
            useLayerMask = true,
        };
    }

    bool _MarkFall = false;

    [ShowInInspector]
    float _FallingHeight = 0;

    public float MaxHeight = 0;

    void Update()
    {
        if (_JumpGracePeriodTimer.Finished && _JumpGracePeriodTimer.Duration != JumpGracePeriod)
        {
            _JumpGracePeriodTimer = new Timer(JumpGracePeriod);
        }

        if (State == MovementState.Frozen)
        {
            Animator.SetBool("falling", false);
            Animator.SetBool("walk", false);
            Animator.SetBool("idle", true);
            return;
        }

        if (!IsGrounded && !Animator.GetBool("falling"))
        {
            Animator.SetBool("falling", true);
        }
        else
        {
            Animator.SetBool("falling", false);
        }

        if (IsGrounded && Direction != 0 && !_IsJumping && !Animator.GetBool("walk"))
        {
            Animator.SetBool("idle", false);
            Animator.SetBool("walk", true);
        }
        else if (
            IsGrounded
            && !_IsJumping
            && !Animator.GetBool("idle")
            && math.length(Velocity.x) <= 0.4
        )
        {
            Animator.SetBool("walk", false);
            Animator.SetBool("idle", true);
        }
    }

    const float k_GroundedJumpCheckDelay = 0.2f;
    float _GroundedJumpCheckTimer = 0f;

    void FixedUpdate()
    {
        HorizontalMovement();
        CollisionChecks();
        CheckFallingHeight();

        if (!_JumpGracePeriodTimer.Finished)
            _JumpGracePeriodTimer.Tick(Time.deltaTime.Secs());
    }

    private void CollisionChecks()
    {
        CheckIsGrounded();
    }

    private void CheckIsGrounded()
    {
        var boxCastOrigin = new Vector2(_Feet.bounds.center.x, _Feet.bounds.min.y);
        var boxCastSize = new Vector2(_Feet.bounds.size.x, _SurfaceAnchor);

        _GroundHit = Physics2D.BoxCast(
            boxCastOrigin,
            boxCastSize,
            0f,
            Vector2.down,
            0.25f,
            GroundLayer
        );

        _IsGrounded = _GroundHit.collider != null;
    }

    bool _MarkFallingHeight = false;

    public void CheckFallingHeight()
    {
        if (_IsJumping)
        {
            _GroundedJumpCheckTimer += Time.deltaTime;
        }

        if (!IsGrounded && !_MarkFall && _IsJumping)
        {
            _JumpGracePeriodTimer.Reset();
            _MarkFall = true;
        }

        if (IsFalling && !_MarkFallingHeight)
        {
            _MarkFallingHeight = true;
            _FallingHeight = transform.position.y;
            MaxHeight = Math.Max(_FallingHeight, MaxHeight);
        }

        if (IsGrounded && _MarkFall)
        {
            _MarkFall = false;
            _MarkFallingHeight = false;
            var height = _FallingHeight - transform.position.y;
            if (height > 0)
                OnFell?.Invoke(height);
            Animator.SetTrigger("land");
            var obj = Instantiate(PuffParticlePrefab);
            obj.transform.position = PuffParticleSpawnPoint.position with { z = -5 };
            _FallingHeight = 0;
        }

        if (
            _IsJumping
            && IsGrounded
            && !IsFalling
            && _GroundedJumpCheckTimer > k_GroundedJumpCheckDelay
        )
        {
            _IsJumping = false;
            _JumpGracePeriodTimer.Tick(_JumpGracePeriodTimer.Duration);
            _UsedJumps = 0;
        }
    }

    public void Stop()
    {
        _Rigidbody.linearVelocityX = 0f;
    }

    private void HorizontalMovement()
    {
        if (State == MovementState.Frozen)
        {
            Direction = 0;
        }
        Slide();

        if (State != MovementState.Frozen)
        {
            TurnCheck(Direction);
        }
        else
        {
            TurnCenter();
            Stop();
        }
    }

    private void Slide()
    {
        var downMod = IsGrounded && !_IsJumping ? 2f : 1f;
        var gravity = Time.deltaTime * GravityFactor * Physics2D.gravity * downMod;
        Velocity += gravity;

        var slideResults = _Rigidbody.Slide(_Velocity, Time.deltaTime, _SlideMovement);

        if (slideResults.slideHit)
        {
            var angle = Vector2.Angle(slideResults.slideHit.normal, Vector2.up);
            var old = Velocity.y;
            Velocity = ClipVector(_Velocity, slideResults.slideHit.normal);
            if (angle >= _MaxSlop && !IsGrounded && !_IsJumping)
            {
                _Velocity.x = 0;
                _Velocity.y = old + gravity.y / 2;
                _Rigidbody.Slide(_Velocity, Time.deltaTime, _SlideMovement);
            }
        }
        // else if (_Velocity.y <= 0 && slideResults.surfaceHit && !_IsJumping)
        // {
        //     var surfaceHit = slideResults.surfaceHit;
        //     Velocity = ClipVector(_Velocity, surfaceHit.normal);
        // }
    }

    private static Vector2 ClipVector(Vector2 vector, Vector2 hitNormal)
    {
        return vector - Vector2.Dot(vector, hitNormal) * hitNormal;
    }

    private void TurnCheck(float moveInput)
    {
        if ((IsFacingRight || _TurnedCenter) && moveInput < 0f)
        {
            Turn(false);
        }
        else if ((!IsFacingRight || _TurnedCenter) && moveInput > 0f)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            IsFacingRight = true;
            ModelPivot.DORotate(new Vector3(0f, 0f, 0f), 0.33f);
        }
        else
        {
            IsFacingRight = false;
            ModelPivot.DORotate(new Vector3(0f, 180f, 0f), 0.33f);
        }
        _TurnedCenter = false;
    }

    bool _TurnedCenter = false;

    private void TurnCenter()
    {
        if (!_TurnedCenter)
            ModelPivot.DORotate(new Vector3(0f, 90f, 0f), 0.33f);
        _TurnedCenter = true;
    }

    bool _IsJumping = false;

    [ShowInInspector]
    int _UsedJumps = 0;
    const int k_TotalJumps = 2;

    public void Jump()
    {
        if ((IsGrounded || !_JumpGracePeriodTimer.Finished) && _UsedJumps < k_TotalJumps)
        {
            _IsJumping = true;
            const float k_DoubleJumpMod = 0.8f;
            const float k_DoubleJumpModTest = 0.8f;

            var force =
                _JumpForce
                * (
                    _UsedJumps <= 0
                        ? 1f
                        : (
                            _JumpGracePeriodTimer.Fract <= 0.5
                                ? k_DoubleJumpModTest
                                : k_DoubleJumpMod
                        )
                );

            if (_UsedJumps == 0)
            {
                Velocity = new Vector2(_Velocity.x, force);
            }
            else
            {
                Velocity = new Vector2(_Velocity.x, _Velocity.y + force);
            }

            if (_UsedJumps == 0)
            {
                _GroundedJumpCheckTimer = 0;
            }
            Animator.SetTrigger("jump");
            var obj = Instantiate(PuffParticlePrefab);
            obj.transform.position = PuffParticleSpawnPoint.position with
            {
                y = PuffParticleSpawnPoint.position.y + 0.15f,
                z = -5
            };
            _UsedJumps++;
            OnJump?.Invoke();
        }
    }
}
