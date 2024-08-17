[CreateAssetMenu(menuName = "GMTK2024 / Card")]
public class Card : ScriptableObject
{
    public string CardName = "Name";

    public Texture2D IconAsset;

    [EnumFlags]
    public CardAction Action = CardAction.Spawn;

    public virtual Option<CardAction> OverrideAction => None;

    [SerializeReference]
    [HideIf(nameof(IsNotSpawn))]
    public Block? Block;

    public bool IsNotSpawn() => !((Action & CardAction.Spawn) != 0);

    public void OnEnable()
    {
        SetAction();
    }

    public void OnValidate()
    {
        SetAction();
    }

    private void SetAction()
    {
        if (OverrideAction.IsSome(out var action))
        {
            Action = action;
        }
    }

    public Block? Activate(Block? target)
    {
        // Check the action flags
        if ((Action & CardAction.Spawn) != 0)
        {
            // Handle Block action
            return DoSpawn();
        }

        if ((Action & CardAction.Morph) != 0)
        {
            // Handle Morph action
            if (target != null)
            {
                DoMorph(target);
            }
            return null;
        }

        if ((Action & CardAction.Action) != 0)
        {
            // Handle Action
            if (target != null)
            {
                DoAction(target);
            }
            return null;
        }

        // If no action is set, return the target unchanged
        return null;
    }

    public virtual Block DoSpawn()
    {
        assert(Block.IsNotNull(), "Block cannot be null");
        return Block!;
    }

    public virtual void DoMorph(Block target) { }

    public virtual void DoAction(Block block) { }
}

[Flags]
public enum CardAction
{
    Spawn = 1 << 0,
    Morph = 1 << 1,
    Action = 1 << 2
}
