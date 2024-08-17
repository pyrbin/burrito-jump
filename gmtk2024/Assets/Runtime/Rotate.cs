using Unity.VisualScripting;

public class Rotate : MonoBehaviour
{
    public float RotationSpeed = 15f;

    public void Update()
    {
        transform.Rotate(Vector3.up * RotationSpeed * Time.deltaTime);
    }
}
