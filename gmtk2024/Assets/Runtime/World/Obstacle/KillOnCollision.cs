using UnityEngine;

public class KillOnCollision : MonoBehaviour
{
    public PhysicsEvents2D events2D;
    private Player _Player;

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
            Vector2 direction = (other.transform.position - transform.position).normalized;
            _Player
                .MovementController.GetComponent<Rigidbody2D>()
                .AddForce(-direction * 8f * Time.deltaTime, ForceMode2D.Impulse);
            LevelManager.Instance.RemoveObstacle(this.GetComponent<Obstacle>());
            Destroy(this.gameObject);
        }
    }
}
