using UnityEngine.UI;

public class PlatformingHUD : MonoSingleton<PlatformingHUD>
{
    public HealthIconUI Health1;
    public HealthIconUI Health2;
    public HealthIconUI Health3;

    public Button RestartButton;

    public void Show()
    {
        Health1.transform.parent.gameObject.SetActive(true);
        RestartButton.gameObject.SetActive(true);
    }

    public void Hide(bool showRestart = false)
    {
        Health1.transform.parent.gameObject.SetActive(false);
        RestartButton.gameObject.SetActive(showRestart);
    }

    public void Start()
    {
        RestartButton.onClick.AddListener(() =>
        {
            if (
                GameManager.Instance.GameState != GameState.StartMenu
                && GameManager.Instance.GameState != GameState.Upgrades
            )
            {
                GameManager.Instance.Restart();
                GameManager.Instance.SetGameState(GameState.Building);
            }
        });
    }

    public void SyncPlayerHealthValues(int health)
    {
        Health1.SetIsActive(health >= 1);
        Health2.SetIsActive(health >= 2);
        Health3.SetIsActive(health >= 3);
    }

    public void Update()
    {
        SyncPlayerHealthValues(Player.Instance.Health);
    }
}
