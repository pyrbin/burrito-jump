[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    [Serializable]
    public enum MovementState
    {
        Frozen,
        Free
    }

    [SerializeField]
    private Collider2D _Feet;
    private RaycastHit2D _GroundHit;

    [ReadOnly]
    public bool _IsGrounded;

    [SerializeField]
    private Collider2D _Body;

    public float JumpForce = 7f;
    public float MaxSpeed = 5f;
    public float Acceleration = 15f;
    public float Deacceleration = 20f;
    public float MaxFallingSpeed = 20f;
    public float AirAccelerationFactor = 0.75f;
    public Duration JumpGracePeriod = 0.33f.Secs();
    public LayerMask GroundLayer;

    public bool IsFacingRight { get; private set; } = true;

    public float Direction
    {
        get => MovementDirection;
        set => MovementDirection = value;
    }

    private Rigidbody2D _Rigidbody;
    private float MovementDirection { get; set; }

    [SerializeField]
    public MovementState State = MovementState.Free;

    public bool IsMovingSideways => math.abs(_Rigidbody.linearVelocity.x) > 0;

    public bool IsAcceleratingSideways => MovementDirection != 0;

    public bool IsGrounded => _IsGrounded;

    public bool IsFalling => _Rigidbody.linearVelocity.y < 0 && !IsGrounded;

    public float2 Velocity => _Rigidbody.linearVelocity;

    public Vector2 MovingDirection => new(MovementDirection, 0);

    private Timer _JumpGracePeriodTimer;

    // events.

    public event Action? OnJump;
    public event Action<float>? OnFell;

    [Header("Debugging")]
    public bool DrawGizmos = true;

    void Awake()
    {
        IsFacingRight = true;
        _JumpGracePeriodTimer = new Timer(JumpGracePeriod);
        _JumpGracePeriodTimer.Tick(JumpGracePeriod);
        TryGetComponent(out _Rigidbody);
    }

    bool _MarkFall = false;
    float _FallingHeight = 0;

    void Update()
    {
        if (_JumpGracePeriodTimer.Finished && _JumpGracePeriodTimer.Duration != JumpGracePeriod)
        {
            _JumpGracePeriodTimer = new Timer(JumpGracePeriod);
        }
    }

    void FixedUpdate()
    {
        CollisionChecks();

        UpdateMovement();

        CheckFallingHeight();

        if (!_JumpGracePeriodTimer.Finished)
            _JumpGracePeriodTimer.Tick(Time.fixedDeltaTime.Secs());
    }

    public void CheckFallingHeight()
    {
        if (IsFalling && !_MarkFall)
        {
            _JumpGracePeriodTimer.Reset();
            _MarkFall = true;
            _FallingHeight = transform.position.y;
        }

        if (IsGrounded && _MarkFall)
        {
            _MarkFall = false;
            var height = _FallingHeight - transform.position.y;
            if (height > 0)
                OnFell?.Invoke(height);
            _FallingHeight = 0;
        }

        if (_IsJumping && IsGrounded && _JumpGracePeriodTimer.Finished)
        {
            _IsJumping = false;
            _UsedJumps = 0;
        }
    }

    public void Stop()
    {
        _Rigidbody.linearVelocityX = 0f;
    }

    private void UpdateMovement()
    {
        if (State != MovementState.Frozen)
        {
            TurnCheck(MovementDirection);

            if (MovementDirection != 0)
            {
                Accelerate();
            }

            Deaccalerate();

            _Rigidbody.linearVelocityY = math.clamp(
                _Rigidbody.linearVelocityY,
                -MaxFallingSpeed,
                50f
            );
        }
        else
        {
            Stop();
        }
    }

    private void TurnCheck(float moveInput)
    {
        if (IsFacingRight && moveInput < 0f)
        {
            Turn(false);
        }
        else if (!IsFacingRight && moveInput > 0f)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            IsFacingRight = true;
            transform.DORotate(new Vector3(0f, 0f, 0f), 0.33f);
        }
        else
        {
            IsFacingRight = false;
            transform.DORotate(new Vector3(0f, 180f, 0f), 0.33f);
        }
    }

    bool _IsJumping = false;
    int _UsedJumps = 0;
    const int k_TotalJumps = 2;

    public void Jump()
    {
        if ((IsGrounded || !_JumpGracePeriodTimer.Finished) && _UsedJumps < k_TotalJumps)
        {
            _IsJumping = true;
            _Rigidbody.AddForce(new float2(0, JumpForce), ForceMode2D.Impulse);
            _UsedJumps++;
            OnJump?.Invoke();
        }
    }

    private void Accelerate()
    {
        var movementDir = new Vector2(MovementDirection, 0);
        _Rigidbody.linearVelocity +=
            movementDir
            * (Acceleration * (IsFalling ? AirAccelerationFactor : 1f))
            * Time.fixedDeltaTime;
        if (math.abs(_Rigidbody.linearVelocity.x) > MaxSpeed)
        {
            _Rigidbody.linearVelocity = new Vector2(
                _Rigidbody.linearVelocity.x * (MaxSpeed / _Rigidbody.linearVelocity.magnitude),
                _Rigidbody.linearVelocity.y
            );
        }
    }

    private void Deaccalerate()
    {
        var movementDir = new Vector2(MovementDirection, 0);
        var decrease = Time.fixedDeltaTime * Deacceleration;

        var xVelocity = _Rigidbody.linearVelocity.x;
        if (xVelocity > 0 && movementDir.x <= 0)
        {
            xVelocity -= decrease;
            xVelocity = Mathf.Clamp(xVelocity, 0f, MaxSpeed);
        }
        if (xVelocity < 0 && movementDir.x >= 0)
        {
            xVelocity += decrease;
            xVelocity = Mathf.Clamp(xVelocity, -MaxSpeed, 0);
        }

        _Rigidbody.linearVelocity = new Vector2(xVelocity, _Rigidbody.linearVelocity.y);
    }

    private void CollisionChecks()
    {
        CheckIsGrounded();
    }

    private void CheckIsGrounded()
    {
        var boxCastOrigin = new Vector2(_Feet.bounds.center.x, _Feet.bounds.min.y);
        var boxCastSize = new Vector2(_Feet.bounds.size.x, 0.5f);

        _GroundHit = Physics2D.BoxCast(
            boxCastOrigin,
            boxCastSize,
            0f,
            Vector2.down,
            0.5f,
            GroundLayer
        );
        _IsGrounded = _GroundHit.collider != null;
    }
}
