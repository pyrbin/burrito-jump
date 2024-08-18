using FMOD.Studio;
using FMODUnity;

public static class FMODUtil
{

    public static void PlayOneShot(EventReference eventRef)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventRef);
        eventInstance.start();
        eventInstance.release();
    }

    public static void PlayIfElseStop(bool shouldPlay, EventInstance eventInstance)
    {
        if (!shouldPlay)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            return;
        }
        
        eventInstance.getPlaybackState(out var state);
        if (state == PLAYBACK_STATE.STOPPING || state == PLAYBACK_STATE.STOPPED)
        {
            eventInstance.start();
        }
    }
}