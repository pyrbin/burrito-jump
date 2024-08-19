using FMODUnity;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Block : MonoBehaviour
{
    public float fallSpeed = 5f;
    public bool _IsFalling = false;
    private Rigidbody2D _Rb;

    public event Action<Block>? OnStartFalling;
    public event Action<Block>? OnStopFalling;

    public EventReference EntryReference;
    public EventReference StopReference;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        _Rb = GetComponent<Rigidbody2D>();
        _Rb.gravityScale = 0;
        _Rb.mass = 0;
        _Rb.simulated = false;
        _Rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void StartFalling()
    {
        OnStartFalling?.Invoke(this);
        FMODUtil.PlayOneShot(EntryReference);
        _Rb.linearVelocity = new Vector2(0, -fallSpeed);
        _Rb.bodyType = RigidbodyType2D.Dynamic;
        _IsFalling = true;
    }

    public void StopFalling()
    {
        OnStopFalling?.Invoke(this);
        FMODUtil.PlayOneShot(StopReference);
        _Rb.linearVelocity = new Vector2(0, 0);
        _Rb.bodyType = RigidbodyType2D.Static;
        _IsFalling = false;
    }

    public void IgnoreCollision(bool ignore)
    {
        if (_Rb == null)
            return;
        _Rb.simulated = !ignore;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (_IsFalling && !collision.gameObject.CompareTag("Pillar"))
        {
            StopFalling();
        }
    }
}
