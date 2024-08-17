public class BlockProvider : MonoBehaviour
{
    public BuildingController buildingController;
    public List<Block> blocks;
    void Start() {
        SetNewBlock();
        buildingController.OnDrop += SetNewBlock;
    }
    private void SetNewBlock() {
        buildingController.SetCurrentBlock(NextBlock());
    }
    public Block NextBlock()
    {
        var randomIndex = Random.Range(0, blocks.Count);
        var selectedBlock = blocks[randomIndex];
        var gameObject = Instantiate(selectedBlock);
        return gameObject;
    }
}