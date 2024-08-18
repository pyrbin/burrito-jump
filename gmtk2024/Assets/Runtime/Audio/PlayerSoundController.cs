using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    public EventReference JumpEvent;
    public EventReference LandingEvent;

    public EventReference RunEvent;
    public EventInstance Run;

    public EventReference FallingEvent;
    public EventInstance Falling;

    public EventReference FallingScreamEvent;
    public EventInstance FallingScream;

    public MovementController movementController;

    void Start()
    {
        movementController.OnFell += _ => FMODUtil.PlayOneShot(LandingEvent);
        movementController.OnJump += () => FMODUtil.PlayOneShot(JumpEvent);

        Run = RuntimeManager.CreateInstance(RunEvent);
        Falling = RuntimeManager.CreateInstance(FallingEvent);
        FallingScream = RuntimeManager.CreateInstance(FallingScreamEvent);

        GameManager.Instance.Player.HealthZero += () => FMODUtil.PlayOneShot(FallingScreamEvent);
    }

    void Update()
    {   
        FMODUtil.PlayIfElseStop(movementController.IsMovingSideways, Run);
    }


}
