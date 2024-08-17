using gmtk2024.Runtime.Stat;
using mote.Runtime.Input;

public class Player : MonoBehaviour
{
    public int Health = 3;
    public MovementController MovementController;
    public BuildingController BuildingController;

    public List<Block> Deck = new();

    [ReadOnly]
    public List<Block> CurrentRunDeck = new();

    [ReadOnly]
    public List<Block> ActiveDeck = new();

    public int ActiveDeckCount => ActiveDeck.Count;

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
        ActiveDeck.AddRange(CurrentRunDeck);
        ActiveDeck.Shuffle();
    }

    public void AddToDeck(Block block)
    {
        CurrentRunDeck.Add(block);
    }

    public Option<Block> TakeBlockFromDeck()
    {
        if (ActiveDeck.Count == 0)
        {
            return None;
        }

        var block = ActiveDeck[ActiveDeck.Count - 1];
        ActiveDeck.RemoveAt(ActiveDeck.Count - 1);
        return block;
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
