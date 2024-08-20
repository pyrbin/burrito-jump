using gmtk2024.Runtime.Stat;
using mote.Runtime.Input;
using Unity.VisualScripting;

public class Player : MonoSingleton<Player>
{
    public int Health = 3;
    public MovementController MovementController;
    public BuildingController BuildingController;
    public CardHolderUI CardHolderUI;
    public int DamageDivider = 15;
    public Transform BloodParticleSpawnPoint;
    public GameObject BloodParticlePrefab;

    public List<Card> Deck = new();

    [ReadOnly]
    public List<Card> CurrentRunDeck = new();

    [ReadOnly]
    public List<Card> ActiveDeck = new();

    [ReadOnly]
    public List<Card> Hand = new();

    public int ActiveDeckCount => ActiveDeck.Count;

    public event Action<Card>? UsedCard;
    public event Action? HealthZero;
    public event Action<int>? TookDamage;
    public event Action? DiscardedCard;

    public int UsedCards = 0;

    int _MaxHealth = 3;

    [Button("Deal Damage")]
    public void DealDamage()
    {
        if (GameManager.Instance.GameState != GameState.Platforming)
            return;
        var damage = 1;
        Health -= damage;
        Health = Math.Clamp(Health, 0, _MaxHealth);
        TookDamage?.Invoke(damage);
        if (Health == 0)
        {
            HealthZero?.Invoke();
        }
    }

    public void Start()
    {
        _MaxHealth = Health;
        SetupEvents();
    }

    public void SetupEvents()
    {
        MovementController.OnFell += (height) =>
        {
            if (GameManager.Instance.GameState != GameState.Platforming)
                return;
            var damage = (int)Mathfs.Clamp(height / DamageDivider, 0, Health);
            if (damage == 0)
                return;
            TakeDamage(damage);
        };
    }

    public void RefillDeck()
    {
        ResetHand();
        CardHolderUI.Sync(new List<Card>());
        CardHolderUI.Reset();
        ActiveDeck.Clear();
        ActiveDeck.AddRange(CurrentRunDeck);
        ActiveDeck.Shuffle();
    }

    public void AddToDeck(Card card)
    {
        CurrentRunDeck.Add(card.Clone());
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (damage > 0)
        {
            var obj = Instantiate(BloodParticlePrefab);
            obj.transform.position = BloodParticleSpawnPoint.position with { z = -5 };
        }

        TookDamage?.Invoke(damage);
        if (Health == 0)
        {
            HealthZero?.Invoke();
        }
    }

    public void ResetHand()
    {
        Hand.Clear();
        CardHolderUI.Sync(Hand);
        CardHolderUI.Reset();
    }

    public void DiscardCardFromHand(Card card)
    {
        Hand.Remove(card);
        CardHolderUI.Sync(Hand);

        if (GameManager.Instance.GameState == GameState.Upgrades)
        {
            CurrentRunDeck.Remove(card);
            CardHolderUI.Sync(Hand);
        }
        else
        {
            UsedCards++;
            DiscardedCard?.Invoke();
        }
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
        UsedCards++;
        UsedCard?.Invoke(card);
    }

    public void Restart()
    {
        CurrentRunDeck.Clear();
        CurrentRunDeck.AddRange(Deck.Select(x => x.Clone()));
        UsedCards = 0;
        MovementController.MaxHeight = 0;
        Health = _MaxHealth;
        RefillDeck();
    }

    void Update()
    {
        if (CardHolderUI.SpawnedCards.Count != Hand.Count)
        {
            CardHolderUI.Sync(Hand);
        }

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
