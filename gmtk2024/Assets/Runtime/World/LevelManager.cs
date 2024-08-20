using pyr.Shared.Common;

[ExecuteInEditMode]
public class LevelManager : MonoSingleton<LevelManager>
{
    [Header("References")]
    public PillarBounds LeftPillar;
    public Transform LeftSpawnBounds;

    public PillarBounds RightPillar;
    public Transform RightSpawnBounds;
    public ParticleSystem Leaves;

    public Goal GoalObject;
    public Transform GoalPosition;

    public Obstacle ObstaclePrefab;

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
    public float CurrentLevelHeight => Level * LevelHeight + LevelAddition;

    public float LevelAddition => Math.FloorToInt(Level / 5) * 3;

    [ShowInInspector]
    public Bounds SpawnBounds
    {
        get
        {
            var minY = LastLevelHeight + 2;
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

    [ReadOnly]
    public List<Obstacle> Obstacles = new();

    void Start()
    {
        SetupEvents();
    }

    public async void Restart()
    {
        foreach (var block in Blocks)
        {
            if (block != null)
                Destroy(block.gameObject);
        }
        foreach (var obstacle in Obstacles)
        {
            if (obstacle != null)
                Destroy(obstacle.gameObject);
        }
        Blocks = new List<Block>();
        Obstacles = new List<Obstacle>();
        Leaves.Stop();

        Level = 1;
        LastLevelHeight = 0;
        Height = CurrentLevelHeight;

        SpawnObstacles();

        await UniTask.Delay(222);

        Character.Instance.transform.position = new Vector3(-7f, 1.7f, 0);
    }

    public void RemoveBlock(Block block)
    {
        Blocks.Remove(block);
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
        if (Level >= 3)
        {
            Leaves.Play();
        }

        SpawnObstacles();

        await UniTask.Delay(millisecondsDelay: 500);

        GoalObject.Enabled = true;
    }

    void SpawnObstacles()
    {
        const bool k_DebugAlwaysSpawm = false;

        const int k_LevelThreshold = 3;
        if (!k_DebugAlwaysSpawm && Level < k_LevelThreshold)
            return;
        var count = k_DebugAlwaysSpawm
            ? 3
            : Random.Range(Math.Clamp01(Level - 6), Math.Clamp(Level - 3, 0, 4));
        if (Level == k_LevelThreshold)
        {
            count = 1;
        }
        for (var i = 0; i < count; i++)
        {
            var second = Random.Range(0f, 1f);
            if (!k_DebugAlwaysSpawm && second < 0.4f)
                return;
            var pos = GetRandomPositionInBounds(SpawnBounds);
            var obs = Instantiate(ObstaclePrefab, pos, Quaternion.identity);
            Obstacles.Add(obs);
        }
    }

    public void RemoveObstacle(Obstacle obstacle)
    {
        Obstacles.Remove(obstacle);
    }

    Vector3 GetRandomPositionInBounds(Bounds bounds)
    {
        var randomX = Random.Range(bounds.min.x, bounds.max.x);
        var randomY = Random.Range(bounds.min.y, bounds.max.y);

        var z = -5;

        // Return the random position
        return new Vector3(randomX, randomY, z);
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
