using mote.Runtime.Input;

public class Player : MonoBehaviour
{
    public MovementController MovementController;
    public BuildingController BuildingController;

    public List<Block> Deck = new();

    [ReadOnly]
    public List<Block> ActiveDeck = new();

    public void RefillDeck()
    {
        ActiveDeck.AddRange(Deck);
        ActiveDeck.Shuffle();
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
