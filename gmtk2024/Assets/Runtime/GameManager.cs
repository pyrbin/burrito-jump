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
    public LevelManager LevelManager => LevelManager.Instance;
    public Character Character => Character.Instance;

    public List<Card> Upgrades = new();

    public CameraManager cameraManager;

    public Player Player;

    public GameInput Input => _GameInput;

    public event Action<GameState>? OnGameStateChanged;
    public event Action OnUpgradeComplete;

    void OnEnable()
    {
        _GameInput ??= new GameInput();
        _GameInput.Enable();
        _GameInput.Gameplay.Disable();
        _GameInput.Building.Disable();
    }

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        // WHY IS THE GAME FRAMERATE DEPENDENT WAT
        Application.targetFrameRate = 144;
    }

    void Start()
    {
        SetupEvents();

        Restart();

        SetGameState(GameState);
    }

    void SetupEvents()
    {
        BuildingController.OnCurrentBlockStopped += OnBlockStopped;
        LevelManager.FinishedLevel += () =>
        {
            MovementController.State = MovementController.MovementState.Frozen;
            SetGameState(GameState.Upgrades);
        };
        Player.HealthZero += OnDeath;
        Player.DiscardedCard += OnDiscard;
        Player.UsedCard += OnUsedCard;
    }

    void Update()
    {
        // if (GameState == GameState.Building && Player.Hand.Count < 3 && Player.ActiveDeckCount > 0)
        // {
        //     Player.DrawToHand(3);
        // }
    }

    public async void OnUsedCard(Card card)
    {
        if (!CheckHandStatus())
        {
            await UniTask.Delay(500);
            Player.DrawToHand(1);
        }
    }

    public async void OnDiscard()
    {
        if (!CheckHandStatus())
        {
            await UniTask.Delay(500);
            Player.DrawToHand(1);
        }
    }

    public async void OnDeath()
    {
        MovementController.State = MovementController.MovementState.Frozen;
        SetInputState(InputState.Menu);

        await UniTask.Delay(500);

        SetGameState(GameState.GameOver);
    }

    public bool CheckHandStatus()
    {
        if (
            BuildingController.currentBlock == null
            && (Player.ActiveDeckCount <= 0 && Player.Hand.Count <= 0)
        )
        {
            SetGameState(GameState.Platforming);
            return true;
        }
        return false;
    }

    public void Restart()
    {
        LevelManager.Restart();
        BuildingController.Restart();
        Player.Restart();
    }

    public void SetGameState(GameState gameState)
    {
        OnExitGameState(GameState);
        LastGameState = GameState;
        GameState = gameState;
        OnGameStateChanged?.Invoke(gameState);
        OnEnterGameState(gameState);
    }

    async void OnEnterGameState(GameState gameState)
    {
        switch ((gameState, LastGameState))
        {
            case (GameState.StartMenu, _):
                StartMenuUI.Instance.Show();
                GameOverUI.Instance.Hide();
                PlatformingHUD.Instance.Hide(false);
                Player.Instance.CardHolderUI.Hide();
                SetInputState(InputState.Menu);
                break;
            case (GameState.Building, _):
                GameOverUI.Instance.Hide();
                PlatformingHUD.Instance.Hide(false);
                Player.Instance.CardHolderUI.Show();
                LevelManager.ShowGoalObject();
                BuildingController.Enable();
                Player.RefillDeck();
                SetInputState(InputState.Building);
                cameraManager?.SwitchToCamera("BuildCamera");
                await UniTask.Delay(222);
                Player.DrawToHand(3);
                break;
            case (GameState.Platforming, _):
                GameOverUI.Instance.Hide();
                PlatformingHUD.Instance.Show();
                Player.ResetHand();
                MovementController.State = MovementController.MovementState.Free;
                LevelManager.ShowGoalObject();
                SetInputState(InputState.Platforming);
                cameraManager?.SwitchToCamera("PlatformCamera");
                break;
            case (GameState.Upgrades, _):
                GameOverUI.Instance.Hide();
                PlatformingHUD.Instance.Hide(false);
                Player.Instance.CardHolderUI.Show();
                cameraManager?.SwitchToCamera("BuildCamera");
                LevelManager.HideGoalObject();
                SetInputState(InputState.Menu);
                CardUI.s_EnableUpgrade = true;
                Player.RefillDeck();
                Player.DrawToHand(3);
                NotificationUI.Instance.ShowMessage(
                    "Choose a card to add to your deck. You can also discard up-to 3 cards.",
                    99999.Secs()
                );
                Upgrades.Shuffle();
                CardUpgradesUI.Instance.Show();
                CardUpgradesUI.Instance.Sync(Upgrades.Take(3).ToList());
                break;
            case (GameState.GameOver, _):
                GameOverUI.Instance.Show(Player.UsedCards, MovementController.MaxHeight);
                PlatformingHUD.Instance.Hide(true);
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
                Player.Instance.CardHolderUI.Hide();
                break;
            case GameState.Platforming:
                MovementController.State = MovementController.MovementState.Frozen;
                break;
            case GameState.Upgrades:
                NotificationUI.Instance.HideMessage();
                CardUpgradesUI.Instance.Hide();
                break;
            case GameState.GameOver:
                GameOverUI.Instance.Hide();
                break;
        }
    }

    public void SetInputState(InputState state)
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

    void OnBlockStopped()
    {
        Player.DrawToHand(1);

        if (Player.ActiveDeckCount <= 0 && Player.Hand.Count <= 0)
        {
            SetGameState(GameState.Platforming);
        }
        else
        {
            SetInputState(InputState.Building);
        }
    }

    public async void AddUpgradeCardToDeck(Card card)
    {
        Player.Instance.AddToDeck(card);
        OnUpgradeComplete?.Invoke();
        CardUI.s_EnableUpgrade = false;
        CardUpgradesUI.Instance.Sync(new List<Card>());

        await UniTask.Delay(222);
        Player.ResetHand();
        SetGameState(GameState.Building);
    }

    #endregion

    public void StartGame()
    {
        SetGameState(GameState.Building);
    }
}
