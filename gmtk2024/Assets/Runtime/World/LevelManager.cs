using pyr.Shared.Common;

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

    public Transform CameraPosition;

    public const f32 k_GoalYOffset = 3f;
    public const f32 k_MapWidth = 20f;

    public event Action? FinishedLevel;

    public int Level = 1;
    public f32 LevelHeight = 15f;

    [Header("Level Scaling")]
    [Range(0f, 200f)]
    public float Height = 15f;

    [ReadOnly]
    public float LastLevelHeight = 0f;
    public float CurrentLevelHeight => Level * LevelHeight;

    [ShowInInspector]
    public Bounds SpawnBounds
    {
        get
        {
            var minY = LastLevelHeight + 1;
            var maxY = CurrentLevelHeight - 4;
            var width = k_MapWidth - 2f;
            var height = maxY - minY;

            var center = new Vector3(0, (minY + maxY) / 2, 0);
            var size = new Vector3(width, height, width);

            return new Bounds(center, size);
        }
    }

    [ReadOnly]
    public List<Block> Blocks = new();

    void Start()
    {
        SetupEvents();
    }

    public void Restart()
    {
        foreach (var block in Blocks)
        {
            if (block != null)
                Destroy(block.gameObject);
        }
        Blocks = new List<Block>();
        Level = 1;
        LastLevelHeight = 0;
        Height = CurrentLevelHeight;
        Character.Instance.transform.position = new Vector3(-7, 1.7f, 0);
    }

    public void ShowGoalObject()
    {
        GoalObject.gameObject.SetActive(true);
        GoalObject.Enabled = true;
    }

    public void HideGoalObject()
    {
        GoalObject.Enabled = false;

        GoalObject.gameObject.SetActive(false);
        GoalObject.Enabled = false;
    }

    void Update()
    {
        UpdatePosition();
        UpdateLevelHeight();

#if UNITY_EDITOR
        var bounds = SpawnBounds;
        DebugDraw.Box(bounds.center, bounds.size * 0.5f, quaternion.identity, Color.red);
#endif
    }

    void SetupEvents()
    {
        GoalObject.ReachedGoal += () =>
        {
            AdvanceLevel();
            FinishedLevel?.Invoke();
        };
    }

    [Button("Reset Level")]
    void ResetLevel()
    {
        Restart();
    }

    [Button("Advance Level")]
    async void AdvanceLevel()
    {
        GoalObject.Enabled = false;

        LastLevelHeight = CurrentLevelHeight;
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
            x = LeftPillar.Collider.bounds.min.x + 2,
            y = LeftPillar.Collider.bounds.max.y
        };

        RightSpawnBounds.transform.position = RightSpawnBounds.transform.position with
        {
            x = RightPillar.Collider.bounds.min.x - 1,
            y = RightPillar.Collider.bounds.max.y
        };

        GoalPosition.transform.position = GoalPosition.transform.position with
        {
            x = 0f,
            y = RightPillar.Collider.bounds.max.y - k_GoalYOffset
        };

        GoalObject.transform.position = GoalPosition.transform.position;

        CameraPosition.transform.position = CameraPosition.transform.position with
        {
            x = 0f,
            y = RightPillar.Collider.bounds.max.y - k_GoalYOffset
        };
    }
}
