using FMODUnity;

public class CardSoundController : MonoBehaviour {


    public EventReference CardSelectReference;
    public EventReference CardHoverReference;

    void Start () {
        CardUI.s_HoverStarted += () => FMODUtil.PlayOneShot(CardHoverReference);
        CardUI.s_StartedDragging += (_) => FMODUtil.PlayOneShot(CardSelectReference);
    }


}