using FMODUnity;
using UnityEngine;

public class AnnouncerSoundController : MonoSingleton<AnnouncerSoundController> 
{
    public EventReference GameEndRef;
    public EventReference GameStartRef;
    public EventReference GameIntroRef;
    public EventReference RandomCommentaryRef;
    public EventReference CheckpointRef;

    void Start() {
        FMODUtil.PlayOneShot(GameIntroRef);
        GameManager.Instance.Player.HealthZero += () => FMODUtil.PlayOneShot(GameEndRef);

    }
    

}
