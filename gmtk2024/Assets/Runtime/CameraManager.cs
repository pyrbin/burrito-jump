using Unity.Cinemachine;

public class CameraManager : MonoSingleton<CameraManager>
{
    public List<CinemachineCamera> cameras;

    void Start() {
        GameManager.Instance.cameraManager = this;   
    }

    public void SwitchToCamera(string cameraName) {
        var camera = cameras.Find(c => c.name == cameraName);
        if (camera.IsNotNull()) {
            cameras.ForEach(c => c.Priority = 0);
            camera.Priority = 1;
        }
    }

}