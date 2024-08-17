using mote.Runtime.Input;

public enum InputState
{
    Building,
    Platforming,
    Menu
}

public class GameManager : MonoSingleton<GameManager>
{
    public InputState InputState { get; private set; } = InputState.Menu;
    private GameInput _GameInput;

    public BuildingController BuildingController;

    public MovementController MovementController;

    public Player Player;

    public GameInput Input => _GameInput;

    void OnEnable()
    {
        _GameInput ??= new GameInput();
        _GameInput.Enable();
        _GameInput.Gameplay.Disable();
        _GameInput.Building.Disable();
    }

    void Start()
    {
        // TODO: start building automatically for now
        StartRound();

        BuildingController.OnCurrentBlockStopped += () => AddBlock();
    }

    void StartRound()
    {
        SetInputState(InputState.Building);
        Player.RefillDeck();
        AddBlock();
    }

    void StartPlatforming()
    {
        SetInputState(InputState.Platforming);
    }

    void AddBlock()
    {
        var maybeBlock = Player.TakeBlockFromDeck();
        if (maybeBlock.IsSome(out var block))
        {
            BuildingController.SetCurrentBlock(block);
        }
    }

    void Update()
    {
        // DEBUG
        if (UnityEngine.Input.GetKeyDown(KeyCode.G))
        {
            SetInputState(
                InputState == InputState.Building ? InputState.Platforming : InputState.Building
            );
        }
    }

    void SetInputState(InputState state)
    {
        InputState = state;
        switch (InputState)
        {
            case InputState.Building:
                _GameInput.Gameplay.Disable();
                _GameInput.Building.Enable();
                break;
            case InputState.Platforming:
                _GameInput.Gameplay.Enable();
                _GameInput.Building.Disable();
                break;
            case InputState.Menu:
                _GameInput.Gameplay.Disable();
                _GameInput.Building.Disable();
                break;
        }
    }
}
