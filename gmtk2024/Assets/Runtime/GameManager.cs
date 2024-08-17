using mote.Runtime.Input;

public enum InputState
{
    Building,
    Platforming,
    Menu
}

public enum GameState
{
    StartMenu,
    Building,
    Platforming,
    Upgrades,
    GameOver
}

public class GameManager : MonoSingleton<GameManager>
{
    public GameState GameState { get; private set; } = GameState.StartMenu;
    public GameState LastGameState { get; private set; } = GameState.StartMenu;

    public InputState InputState { get; private set; } = InputState.Menu;

    private GameInput _GameInput;

    public BuildingController BuildingController;

    public MovementController MovementController;

    public CameraManager cameraManager;

    public Player Player;

    public GameInput Input => _GameInput;

    public event Action<GameState> OnGameStateChanged;

    void OnEnable()
    {
        _GameInput ??= new GameInput();
        _GameInput.Enable();
        _GameInput.Gameplay.Disable();
        _GameInput.Building.Disable();
    }

    void Start()
    {
        SetupEvents();

        SetGameState(GameState);

        // TODO: auto to play
        SetGameState(GameState.Building);
    }

    void SetupEvents()
    {
        BuildingController.OnCurrentBlockStopped += () => AddBlock();
    }

    void Update()
    {
        // DEBUG
        if (UnityEngine.Input.GetKeyDown(KeyCode.G))
        {
            SetGameState(
                GameState == GameState.Building ? GameState.Platforming : GameState.Building
            );
        }
    }

    void SetGameState(GameState gameState)
    {
        OnExitGameState(GameState);
        LastGameState = GameState;
        GameState = gameState;
        OnGameStateChanged?.Invoke(gameState);
        OnEnterGameState(gameState);
    }

    void OnEnterGameState(GameState gameState)
    {
        switch ((gameState, LastGameState))
        {
            case (GameState.StartMenu, _):
                SetInputState(InputState.Menu);
                break;
            case (GameState.Building, _):
                BuildingController.Enable();
                Player.RefillDeck();
                AddBlock();
                SetInputState(InputState.Building);
                cameraManager?.SwitchToCamera("BuildCamera");
                break;
            case (GameState.Platforming, _):
                SetInputState(InputState.Platforming);
                cameraManager?.SwitchToCamera("PlatformCamera");
                break;
            case (GameState.Upgrades, _):
                SetInputState(InputState.Menu);
                break;
            case (GameState.GameOver, _):
                SetInputState(InputState.Menu);
                break;
        }
    }

    private void OnExitGameState(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.StartMenu:
                break;
            case GameState.Building:
                BuildingController.Disable();
                break;
            case GameState.Platforming:
                break;
            case GameState.Upgrades:
                break;
            case GameState.GameOver:
                break;
        }
    }

    private void SetInputState(InputState state)
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

    #region Game Logic

    void AddBlock()
    {
        var maybeBlock = Player.TakeBlockFromDeck();
        if (maybeBlock.IsSome(out var block))
        {
            BuildingController.SetCurrentBlock(block);
        }
    }

    #endregion
}
