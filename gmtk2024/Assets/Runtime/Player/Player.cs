using gmtk2024.Runtime.Stat;
using mote.Runtime.Input;
using Unity.VisualScripting;

public class Player : MonoSingleton<Player>
{
    public int Health = 3;
    public MovementController MovementController;
    public BuildingController BuildingController;
    public CardHolderUI CardHolderUI;

    public List<Card> Deck = new();

    [ReadOnly]
    public List<Card> CurrentRunDeck = new();

    [ReadOnly]
    public List<Card> ActiveDeck = new();

    [ReadOnly]
    public List<Card> Hand = new();

    public int ActiveDeckCount => ActiveDeck.Count;

    public event Action<Card>? UsedCard;

    public void Start()
    {
        SetupEvents();
    }

    public void SetupEvents()
    {
        MovementController.OnFell += (height) =>
        {
            // TODO: deal damage based on height
            Debug.Log(height);
        };
    }

    public void RefillDeck()
    {
        ActiveDeck.Clear();
        ActiveDeck.AddRange(CurrentRunDeck.Select(x => x.Clone()));
        ActiveDeck.Shuffle();
        ResetHand();
    }

    public void AddToDeck(Card card)
    {
        CurrentRunDeck.Add(card);
    }

    public void ResetHand()
    {
        Hand.Clear();
        CardHolderUI.Sync(Hand);
    }

    public void DiscardCardFromHand(Card card)
    {
        Hand.Remove(card);
        CardHolderUI.Sync(Hand);

        UsedCard?.Invoke(card);
    }

    public void DrawToHand(int count)
    {
        if (Hand.Count + count > 3)
            return;
        if (ActiveDeck.Count == 0)
            return;
        var clampedCount = Math.Clamp(count, 0, ActiveDeck.Count);
        Hand.AddRange(ActiveDeck.Take(clampedCount));
        ActiveDeck.RemoveRange(0, clampedCount);

        CardHolderUI.Sync(Hand);
    }

    public void ActivateCard(Card card, Block? block)
    {
        if (!Hand.Contains(card))
            return;
        Hand.Remove(card);
        var spawned = card.Activate(block);
        if (spawned.IsNotNull())
        {
            BuildingController.SetCurrentBlock(spawned);
        }

        CardHolderUI.Sync(Hand);

        UsedCard?.Invoke(card);
    }

    public void Restart()
    {
        CurrentRunDeck.Clear();
        CurrentRunDeck.AddRange(Deck);

        RefillDeck();
    }

    void Update()
    {
        switch (GameManager.Instance.InputState)
        {
            case InputState.Building:

                if (GameManager.Instance.Input.Building.Drop.WasPressedThisFrame())
                {
                    BuildingController.Drop();
                }

                if (GameManager.Instance.Input.Building.RotateLeft.WasPressedThisFrame())
                {
                    BuildingController.RotateLeft();
                }
                else if (GameManager.Instance.Input.Building.RotateRight.WasPressedThisFrame())
                {
                    BuildingController.RotateRight();
                }

                var mousePosition = GameManager.Instance.Input.Building.Move.ReadValue<Vector2>();
                BuildingController.UpdatePosition(Camera.main.ScreenToWorldPoint(mousePosition));

                break;
            case InputState.Platforming:
                var jumpPressed = GameManager.Instance.Input.Gameplay.Jump.WasPressedThisFrame();
                var movement = GameManager.Instance.Input.Gameplay.Move.ReadValue<Vector2>();
                MovementController.Direction = movement.x;
                if (jumpPressed)
                {
                    MovementController.Jump();
                }
                break;
        }
    }
}
