[CreateAssetMenu(menuName = "GMTK2024 / Destroy Card")]
public class DestroyCard : Card
{
    public override Option<CardAction> OverrideAction => CardAction.Action;

    public override Card Clone() => this.CloneScriptableObject<DestroyCard>();

    public override void DoAction(Block block)
    {
        LevelManager.Instance.RemoveBlock(block);
        Destroy(block.gameObject);
    }
}
