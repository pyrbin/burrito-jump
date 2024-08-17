public class BuildingController : MonoBehaviour
{
    public Block? currentBlock;
    public f32 rotationDegrees = 30f; // Rotation angle in degrees
    public BoundToLine boundToLine;
    public bool IsDropping = false;

    public event Action OnDrop;

    public event Action OnCurrentBlockStopped;

    public void SetCurrentBlock(Block block)
    {
        if (IsDropping)
            return;

        var instance = Instantiate(block);
        currentBlock = instance;
        currentBlock.transform.position = boundToLine.minTransform.position;
        currentBlock.IgnoreCollision(true);
    }

    public void Drop()
    {
        if (currentBlock == null || IsDropping)
            return;

        currentBlock.IgnoreCollision(false);
        currentBlock.StartFalling();
        OnDrop?.Invoke();
        IsDropping = true;
        currentBlock.OnStopFalling += (_) =>
        {
            currentBlock = null;
            IsDropping = false;
            OnCurrentBlockStopped?.Invoke();
        };
    }

    public void UpdatePosition(Vector3 pos)
    {
        if (currentBlock == null || IsDropping)
            return;
        boundToLine.UpdatePosition(currentBlock.gameObject, pos);
    }

    public void RotateRight()
    {
        if (currentBlock == null || IsDropping)
            return;
        currentBlock?.transform.Rotate(Vector3.forward, -rotationDegrees);
    }

    public void RotateLeft()
    {
        if (currentBlock == null || IsDropping)
            return;
        currentBlock?.transform.Rotate(Vector3.forward, rotationDegrees);
    }
}
