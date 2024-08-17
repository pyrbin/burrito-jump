[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    [Serializable]
    public enum MovementState
    {
        Frozen,
        Free
    }

    public float JumpForce = 7f;
    public float MaxSpeed = 5f;
    public float Acceleration = 15f;
    public float Deacceleration = 20f;
    public float MaxFallingSpeed = 20f;

    public bool IsFacingRight { get; private set; } = true;

    public float Direction
    {
        get => MovementDirection;
        set => MovementDirection = value;
    }

    private Rigidbody2D _Rigidbody;
    public ContactFilter2D ContactFilter;

    private float MovementDirection { get; set; }

    [SerializeField]
    public MovementState State = MovementState.Free;

    public bool IsMovingSideways => math.abs(_Rigidbody.linearVelocity.x) > 0;

    public bool IsAcceleratingSideways => MovementDirection != 0;

    public bool IsGrounded => _Rigidbody.IsTouching(ContactFilter);

    public bool IsFalling => _Rigidbody.linearVelocity.y < 0 && !IsGrounded;

    public float2 Velocity => _Rigidbody.linearVelocity;

    public Vector2 MovingDirection => new(MovementDirection, 0);

    // events.

    public event Action? OnJump;

    [Header("Debugging")]
    public bool DrawGizmos = true;

    void Awake()
    {
        IsFacingRight = true;

        TryGetComponent(out _Rigidbody);
    }

    void FixedUpdate()
    {
        UpdateMovement();
    }

    public void Stop()
    {
        _Rigidbody.linearVelocity = float2.zero;
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

    public void Jump()
    {
        if (IsGrounded)
        {
            _Rigidbody.AddForce(new float2(0, JumpForce), ForceMode2D.Impulse);
            OnJump?.Invoke();
        }
    }

    private void Accelerate()
    {
        var movementDir = new Vector2(MovementDirection, 0);
        _Rigidbody.linearVelocity += movementDir * Acceleration * Time.fixedDeltaTime;
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
}
