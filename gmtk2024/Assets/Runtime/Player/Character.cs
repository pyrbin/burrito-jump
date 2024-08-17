[RequireComponent(typeof(MovementController))]
public class Character : MonoSingleton<Character>
{
    public MovementController Controller;

    private new void Awake()
    {
        base.Awake();
        Controller = GetComponent<MovementController>();
    }
}
