public class Block : MonoBehaviour
{
    public float fallSpeed = 5f; 
    private bool _IsFalling = true; 
    private Rigidbody2D _Rb; 

    void Start()
    {
        _Rb = GetComponent<Rigidbody2D>(); 
        _Rb.gravityScale = 0;
        _Rb.mass = 0;
        if (_IsFalling) StartFalling();
    }

    public void StartFalling()
    {
        _Rb.linearVelocity = new Vector2(0, -fallSpeed);
        _Rb.bodyType = RigidbodyType2D.Dynamic;
        _IsFalling = true; 
    }

    public void StopFalling() {
        _Rb.linearVelocity = new Vector2(0, 0); 
        _Rb.bodyType = RigidbodyType2D.Kinematic;
        _IsFalling = false; 
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (_IsFalling) 
        {
            StopFalling();
        }
    }
    
}
