public class BoundToLine : MonoBehaviour
{
    public Transform minTransform; 
    public Transform maxTransform;

    public void UpdatePosition(GameObject gameObject, Vector3 pos)
    {
        var x = Mathf.Clamp(pos.x, minTransform.position.x, maxTransform.position.x);
        gameObject.transform.position = new Vector3(x, gameObject.transform.position.y, gameObject.transform.position.z);
    }
}
