
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Block : MonoBehaviour
{
    public float fallSpeed = 5f;
    public bool _IsFalling = false;
    private Rigidbody2D _Rb;
    public event Action<Block> OnStartFalling;

    void Start()
    {
        _Rb = GetComponent<Rigidbody2D>();
        _Rb.gravityScale = 0;
        _Rb.mass = 0;
        _Rb.simulated = false;
        _Rb.bodyType = RigidbodyType2D.Kinematic;
        if (_IsFalling) StartFalling();
    }

    public void StartFalling()
    {
        OnStartFalling?.Invoke(this);
        _Rb.linearVelocity = new Vector2(0, -fallSpeed);
        _Rb.bodyType = RigidbodyType2D.Dynamic;
        _IsFalling = true;
    }

    public void StopFalling()
    {
        _Rb.linearVelocity = new Vector2(0, 0);
        _Rb.bodyType = RigidbodyType2D.Static;
        _IsFalling = false;
    }

    public void IgnoreCollision(bool ignore)
    {
        if (_Rb == null) return;    
        _Rb.simulated = !ignore;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (_IsFalling)
        {
            StopFalling();
        }
    }

}
