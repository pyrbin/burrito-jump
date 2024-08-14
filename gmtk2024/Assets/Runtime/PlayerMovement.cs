using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    void Update()
    {
        const float k_Speed = 90.0f;

        var speed = k_Speed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.W))
        {
            transform.position += new Vector3(0, 0, 1) * speed;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            transform.position += new Vector3(0, 0, -1) * speed;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.position += new Vector3(-1, 0, 0) * speed;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.position += new Vector3(1, 0, 0) * speed;
        }
    }
}
