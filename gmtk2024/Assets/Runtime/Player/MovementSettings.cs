[CreateAssetMenu(menuName = "Player Movement")]
[Serializable]
public class MovementSettings : ScriptableObject
{
    [Header("Walk")]
    public f32 MaxWalkSpeed = 12.5f;
    public f32 GroundAcceleration = 5f;
    public f32 GroundDeceleration = 20f;
    public f32 AirAceleration = 5f;
    public f32 AirDecceleration = 5f;

    [Header("Run")]
    public float MaxRunSpeed = 20f;

    [Header("Grounded / Collision Checks")]
    public LayerMask GroundLayer;
    public f32 GroundDetectionRayLength = 0.02f;
    public f32 HeadDetectionRayLength = 0.02f;
    public f32 HeadWidth = 0.75f;

    [Header("Jump")]
    public f32 JumpHeight = 6.5f;

    [Range(1f, 1.1f)]
    public f32 JumpHeightCompensationFactor = 1.054f;
    public f32 TimeTillJumpApex = 0.35f;

    [Range(0.01f, 5f)]
    public f32 GravityOnReleaseMultiplier = 2f;
    public f32 MaxFallSpeed = 26f;

    [Range(1, 5)]
    public i32 NumberOfJumpsAllowed = 2;

    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)]
    public f32 TimeForUpwardsCancel = 0.027f;

    [Header("Jump Apex")]
    [Range(0.5f, 1f)]
    public f32 ApexThreshold = 0.97f;

    [Range(0.01f, 0.5f)]
    public f32 ApexHangTime = 0.075f;

    [Header("Jump Buffer")]
    [Range(0f, 1f)]
    public float JumpBufferTime = 0.125f;

    [Header("Jump Coyote Time")]
    [Range(0f, 1f)]
    public float JumpCoyoteTime = 0.1f;

    public f32 Gravity { get; private set; }

    public f32 InitialJumpVelocity { get; private set; }

    public f32 AdjustedJumpHeight { get; private set; }

    public void OnValidate()
    {
        CalculateValues();
    }

    public void OnEnable()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        Gravity = -(2f * JumpHeight) / math.pow(TimeTillJumpApex, 2f);
        InitialJumpVelocity = math.abs(Gravity) * TimeTillJumpApex;
    }
}
