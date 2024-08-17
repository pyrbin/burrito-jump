public class PlayerHoldDragIndicator : MonoBehaviour
{
    public SpriteRenderer DragStartIndicator;

    public SpriteRenderer DragEndIndicator;

    public TMPro.TMP_Text Text;

    private LineRenderer _LineRenderer;

    void Awake()
    {
        _LineRenderer = GetComponentInChildren<LineRenderer>();
        _LineRenderer.ResetBounds();
    }

    void Update() { }
}
