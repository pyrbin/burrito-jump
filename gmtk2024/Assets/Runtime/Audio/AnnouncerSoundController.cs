using FMODUnity;

public class AnnouncerSoundController : MonoSingleton<AnnouncerSoundController> 
{
    public EventReference GameEndRef;
    public EventReference GameStartRef;
    public EventReference GameIntroRef;
    public EventReference RandomCommentaryRef;
    public EventReference CheckpointRef;

    private float _CommentaryTimer;
    public float commentaryInterval = 10f; // Interval between random commentary, in seconds

    void Start() {
        FMODUtil.PlayOneShot(GameIntroRef);
        GameManager.Instance.Player.HealthZero += () => FMODUtil.PlayOneShot(GameEndRef);
        _CommentaryTimer = commentaryInterval;
    }

    void Update() {
        if (GameManager.Instance.GameState == GameState.Platforming) {
            _CommentaryTimer -= Time.deltaTime;

            if (_CommentaryTimer <= 0f) {
                PlayRandomCommentary();
                _CommentaryTimer = commentaryInterval;
            }
        }
    }

    private void PlayRandomCommentary() {
        FMODUtil.PlayOneShot(RandomCommentaryRef);
    }
}
