public class BuildingController : MonoBehaviour
{
    public Block? currentBlock;
    public f32 rotationDegrees = 30f; // Rotation angle in degrees
    public BoundToLine boundToLine;
    public bool IsDropping = false;
    public bool IsLoading = false;

    public event Action OnDrop;

    public event Action OnCurrentBlockStopped;

    public void Enable() { }

    public void Disable()
    {
        if (currentBlock.IsNotNull())
        {
            currentBlock.gameObject.SetActive(false);
            Destroy(currentBlock.gameObject);
        }
        currentBlock = null;
    }

    public void Restart() { }

    public async void SetCurrentBlock(Block block)
    {
        if (IsDropping || IsLoading)
            return;
        IsLoading = true;
        var instance = Instantiate(block);
        instance.transform.position = boundToLine.minTransform.position;
        instance.IgnoreCollision(true);
        instance.gameObject.SetActive(false);
        currentBlock = instance;

        await UniTask.Delay(millisecondsDelay: 100);

        currentBlock.Init();
        currentBlock.gameObject.SetActive(true);
        IsLoading = false;
    }

    public void Drop()
    {
        if (currentBlock == null || IsDropping || IsLoading)
            return;

        LevelManager.Instance.Blocks.Add(currentBlock);
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
        if (currentBlock == null || IsDropping || IsLoading)
            return;
        currentBlock?.transform.Rotate(Vector3.forward, -rotationDegrees);
    }

    public void RotateLeft()
    {
        if (currentBlock == null || IsDropping || IsLoading)
            return;
        currentBlock?.transform.Rotate(Vector3.forward, rotationDegrees);
    }
}
