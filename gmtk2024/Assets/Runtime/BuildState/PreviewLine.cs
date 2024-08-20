public class PreviewLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public LayerMask layerMask;
    public BuildingController buildingController;

    void Start()
    {
        lineRenderer = gameObject.GetOrAddComponent<LineRenderer>();
    }

    public void Update()
    {
        lineRenderer.positionCount = 0;
        if (GameManager.Instance.GameState != GameState.Building)
            return;

        if (
            buildingController.IsDropping
            || buildingController.IsLoading
            || buildingController.currentBlock == null
        )
            return;

        var currentBlock = buildingController.currentBlock;
        var origin = currentBlock.transform.position;
        var hit = Physics2D.Raycast(origin, Vector2.down, Mathf.Infinity, layerMask);
        if (hit.collider != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, hit.point);
        }
    }
}
