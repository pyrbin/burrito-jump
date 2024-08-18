public class CardDragIndicator : MonoBehaviour
{
    public SpriteRenderer DragStartIndicator;

    public SpriteRenderer DragEndIndicator;

    public TMPro.TMP_Text Text;

    private LineRenderer _LineRenderer;

    void Awake()
    {
        _LineRenderer = GetComponentInChildren<LineRenderer>();
        _LineRenderer.positionCount = 0;
    }

    public void Start()
    {
        CardUI.s_StartedDragging += DragStart;
        CardUI.s_EndedDragging += DragEnd;
    }

    private float3 _Origin;

    private float3 Current => Camera.main.ScreenToWorldPoint(Input.mousePosition with { z = 0 });

    private bool _Showing = false;

    public void DragStart(float3 mousePos)
    {
        _Showing = true;
        _Origin = Camera.main.ScreenToWorldPoint(mousePos with { z = 0 });
        _LineRenderer.positionCount = 2;
        DragStartIndicator.gameObject.SetActive(true);
        DragEndIndicator.gameObject.SetActive(true);
    }

    public void DragEnd()
    {
        _Showing = false;
        DragStartIndicator.gameObject.SetActive(false);
        DragEndIndicator.gameObject.SetActive(false);
        _LineRenderer.positionCount = 0;
    }

    public void Update()
    {
        if (!_Showing)
            return;
        DragStartIndicator.transform.position = _Origin with { z = -5 };
        DragEndIndicator.transform.position = Current with { z = -5 };
        _LineRenderer.SetPosition(0, _Origin with { z = -5 });
        _LineRenderer.SetPosition(1, Current with { z = -5 });

        if (CardUI.s_OverValidTarget)
        {
            _LineRenderer.startColor = Color.green;
            _LineRenderer.endColor = Color.green;
            DragEndIndicator.color = Color.green;
            DragStartIndicator.color = Color.green;
        }
        else
        {
            _LineRenderer.startColor = Color.white;
            _LineRenderer.endColor = Color.white;
            DragEndIndicator.color = Color.white;
            DragStartIndicator.color = Color.white;
        }
    }
}
