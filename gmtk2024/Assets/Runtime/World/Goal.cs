using Unity.VisualScripting;

public class Goal : MonoBehaviour
{
    public PhysicsEvents2D Events;

    public bool Enabled = true;

    public event Action? ReachedGoal;

    void Awake()
    {
        Events.TriggerEnter += (collider) =>
        {
            if (!Enabled)
                return;
            if (collider.gameObject.CompareTag("Player"))
            {
                ReachedGoal?.Invoke();
                Enabled = false;
            }
        };
    }

    void Update()
    {
        if (gameObject.activeSelf != Enabled)
            gameObject.SetActive(Enabled);
    }
}
