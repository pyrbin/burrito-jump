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
        if (other.gameObject.CompareTag("Player"))
        {
            _Player.TakeDamage(1);
        }

        LevelManager.Instance.RemoveObstacle(this.GetComponent<Obstacle>());
        Destroy(this.gameObject);
    }
}
