public class BuildingController : MonoSingleton<BuildingController>
{
    public Block? currentBlock;
    public Rigidbody2D? currentBlockRigidbody;
    public Block? droppingBlock;
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
        currentBlockRigidbody = instance.GetComponent<Rigidbody2D>();
        await UniTask.Delay(millisecondsDelay: 100);

        currentBlock.Init();
        currentBlock.gameObject.SetActive(true);
        IsLoading = false;
    }

    public void Drop()
    {
        if (currentBlock == null || IsDropping || IsLoading)
            return;

        // GameManager.Instance.SetInputState(InputState.Menu);
        LevelManager.Instance.Blocks.Add(currentBlock);
        currentBlock.IgnoreCollision(false);
        currentBlock.StartFalling();
        OnDrop?.Invoke();
        IsDropping = true;
        currentBlock.OnStopFalling += (_) =>
        {
            currentBlock = null;
            currentBlockRigidbody = null;
            IsDropping = false;
            OnCurrentBlockStopped?.Invoke();
        };
    }

    public void UpdatePosition(Vector3 pos)
    {
        // TODO: Add way to move when its dropping

        if (currentBlock == null || IsLoading || IsDropping)
            return;
        boundToLine.UpdatePosition(currentBlock.gameObject, pos);
    }

    public void RotateRight()
    {
        if (currentBlock == null || IsLoading)
            return;
        currentBlockRigidbody?.transform.Rotate(Vector3.forward, -rotationDegrees);
    }

    public void RotateLeft()
    {
        if (currentBlock == null || IsLoading)
            return;
        currentBlockRigidbody?.transform.Rotate(Vector3.forward, rotationDegrees);
    }
}
