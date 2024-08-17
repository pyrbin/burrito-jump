public class BuildingController : MonoBehaviour
{
    public Block? currentBlock;
    public f32 rotationDegrees = 30f; // Rotation angle in degrees
    public BoundToLine boundToLine;
    public event Action OnDrop;

    public void SetCurrentBlock(Block block)
    {
        currentBlock = block;
        currentBlock.transform.position = boundToLine.minTransform.position;
        currentBlock.IgnoreCollision(true);
    }

    public void Drop()
    {
        if (currentBlock == null) return;
        currentBlock.IgnoreCollision(false);
        currentBlock.StartFalling();
        currentBlock = null;
        OnDrop?.Invoke();
    }

    public void UpdatePosition(Vector3 pos)
    {
        if (currentBlock == null) return;
        boundToLine.UpdatePosition(currentBlock.gameObject, pos);
    }
    
    public void RotateRight()
    {
        currentBlock?.transform.Rotate(Vector3.forward, -rotationDegrees); 
    }

    public void RotateLeft()
    {
        currentBlock?.transform.Rotate(Vector3.forward, rotationDegrees);
    }

}
