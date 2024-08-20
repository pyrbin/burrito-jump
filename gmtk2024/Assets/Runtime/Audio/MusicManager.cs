using FMOD.Studio;
using FMODUnity;

public class MusicManager : MonoSingleton<MusicManager>
{
    public EventReference SoundtrackReference;
    public EventInstance SoundtrackInstance;
    private PARAMETER_ID _LevelParameter;

    void Start()
    {
        SoundtrackInstance = RuntimeManager.CreateInstance(SoundtrackReference);
        SoundtrackInstance.start();
        SoundtrackInstance.getDescription(out var soundtrackEventDescription);
        PARAMETER_DESCRIPTION levelParameterDescription;
        soundtrackEventDescription.getParameterDescriptionByName(
            "Music progression",
            out levelParameterDescription
        );
        _LevelParameter = levelParameterDescription.id;
        SoundtrackInstance.setParameterByID(_LevelParameter, 4);

        GameManager.Instance.LevelManager.FinishedLevel += () =>
            SetMusicLevel(GameManager.Instance.LevelManager.Level);
    }

    public void SetMusicLevel(int level)
    {
        level = Math.Clamp(level, 0, 5);
        SoundtrackInstance.setParameterByID(_LevelParameter, level);
    }

    public void Restart()
    {
        SetMusicLevel(2);
        SoundtrackInstance.getPlaybackState(out var playbackState);
        if (playbackState != PLAYBACK_STATE.PLAYING)
        {
            SoundtrackInstance.start();
        }
    }
}
