using UnityEngine;

public class StartMenuButton : MonoBehaviour
{
    public void StartGame() {
        GameManager.Instance.StartGame();
    }
}
