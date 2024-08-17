[CreateAssetMenu(menuName = "GMTK2024 / Destroy Card")]
public class DestroyCard : Card
{
    public override Option<CardAction> OverrideAction => CardAction.Action;

    public override void DoAction(Block block)
    {
        LevelManager.Instance.RemoveBlock(block);
        DestroyImmediate(block);
    }
}
