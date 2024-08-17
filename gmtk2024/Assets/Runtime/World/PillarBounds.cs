[ExecuteInEditMode]
[RequireComponent(typeof(Collider2D))]
public class PillarBounds : MonoBehaviour
{
    [Range(0f, 50f)]
    public float Size = 5f;

    public float YPos;

    public BoxCollider2D Collider;

    [Button("Save Y Pos")]
    void SaveYPosition()
    {
        YPos = transform.position.y;
    }

    void Start()
    {
        TryGetComponent<BoxCollider2D>(out var Collider);
    }

    void Update()
    {
        transform.localScale = new float3(transform.localScale.x, Size, transform.localScale.z);
        transform.position = transform.position with { y = YPos + Size / 2 };
    }
}
