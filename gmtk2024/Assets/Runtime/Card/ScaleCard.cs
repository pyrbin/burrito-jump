[CreateAssetMenu(menuName = "GMTK2024 / Scale Card")]
public class ScaleCard : Card
{
    public override Option<CardAction> OverrideAction => CardAction.Morph;

    public override Card Clone() => this.CloneScriptableObject<ScaleCard>();

    public f32 ScaleFactor = 1.5f;

    public override void DoMorph(Block block)
    {
        var scaled = block.gameObject.transform.localScale * ScaleFactor;
        block.gameObject.transform.localScale = scaled with
        {
            z = block.gameObject.transform.localScale.z
        };
    }
}
