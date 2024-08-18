using UnityEngine;

public class KillOnCollision : MonoBehaviour
{
    public PhysicsEvents2D events2D;
    private Player _Player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _Player = GameManager.Instance.Player;
        events2D.TriggerEnter += TriggerEnter;
    }

    public void TriggerEnter(Collider2D other)
    {
        Log(other.tag);
        if (other.gameObject.CompareTag("Player"))
        {
            _Player.TakeDamage(_Player.Health);
        }
    }

}
