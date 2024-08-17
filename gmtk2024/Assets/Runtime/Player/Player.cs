using mote.Runtime.Input;

[RequireComponent(typeof(MovementController))]
public class Player : MonoBehaviour
{
    private MovementController Movement;
    private GameInput _GameInput;

    private float2 _MovementInput;
    private bool _JumpPressed = false;
    private bool _JumpReleased = false;

    void OnEnable()
    {
        _GameInput ??= new GameInput();
        _GameInput.Enable();
        _GameInput.Gameplay.Enable();
    }

    void Start()
    {
        TryGetComponent(out Movement);
    }

    void Update()
    {
        _JumpPressed = _GameInput.Gameplay.Jump.WasPressedThisFrame();
        _JumpReleased = _GameInput.Gameplay.Jump.WasReleasedThisFrame();
        _MovementInput = _GameInput.Gameplay.Move.ReadValue<Vector2>();

        Movement.Direction = _MovementInput.x;
        if (_JumpPressed)
        {
            Movement.Jump();
        }
    }
}
