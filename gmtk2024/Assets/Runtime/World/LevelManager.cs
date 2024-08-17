[ExecuteInEditMode]
public class LevelManager : MonoSingleton<LevelManager>
{
    [Header("References")]
    public PillarBounds LeftPillar;
    public Transform LeftSpawnBounds;

    public PillarBounds RightPillar;
    public Transform RightSpawnBounds;

    public Goal GoalObject;
    public Transform GoalPosition;

    public const f32 k_BoundsOffset = 2f;
    public const f32 k_GoalYOffset = 3f;

    public int Level = 1;
    public f32 LevelHeight = 20f;

    [Header("Level Scaling")]
    [Range(0f, 200f)]
    public float Height = 20f;

    [ReadOnly]
    public float LastLevelHeight = 0f;
    public float CurrentLevelHeight => Level * LevelHeight;

    public List<Block> Blocks = new();

    void Start()
    {
        SetupEvents();
    }

    void Update()
    {
        UpdatePosition();
        UpdateLevelHeight();
    }

    void SetupEvents()
    {
        GoalObject.ReachedGoal += () =>
        {
            // TODO: go to shop
            AdvanceLevel();
        };
    }

    async void AdvanceLevel()
    {
        GoalObject.Enabled = false;

        Level++;
        Height = CurrentLevelHeight;

        await UniTask.Delay(millisecondsDelay: 500);

        GoalObject.Enabled = true;
    }

    void UpdateLevelHeight()
    {
        RightPillar.Size = Height;
        LeftPillar.Size = Height;
    }

    void UpdatePosition()
    {
        LeftSpawnBounds.transform.position = LeftSpawnBounds.transform.position with
        {
            x = LeftPillar.Collider.bounds.min.x + k_BoundsOffset,
            y = LeftPillar.Collider.bounds.max.y
        };

        RightSpawnBounds.transform.position = RightSpawnBounds.transform.position with
        {
            x = RightPillar.Collider.bounds.min.x - k_BoundsOffset,
            y = RightPillar.Collider.bounds.max.y
        };

        GoalPosition.transform.position = GoalPosition.transform.position with
        {
            x = 0f,
            y = RightPillar.Collider.bounds.max.y - k_GoalYOffset
        };

        GoalObject.transform.position = GoalPosition.transform.position;
    }
}
