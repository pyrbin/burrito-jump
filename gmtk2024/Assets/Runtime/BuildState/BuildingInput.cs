public class BuildingInput : MonoBehaviour
{
    public BuildingController buildController; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            buildController.RotateRight();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            buildController.RotateLeft();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            buildController.Drop();
        }

        buildController.UpdatePosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }
}
