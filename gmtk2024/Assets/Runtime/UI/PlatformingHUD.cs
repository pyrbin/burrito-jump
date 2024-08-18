using UnityEngine.UI;

public class PlatformingHUD : MonoSingleton<PlatformingHUD>
{
    public List<HealthIconUI> HealthIcons;
    public Button RestartButton;

    public void Show()
    {
        HealthIcons.First().transform.parent.gameObject.SetActive(true);
        RestartButton.gameObject.SetActive(true);
    }

    public void Hide(bool showRestart = false)
    {
        HealthIcons.First().transform.parent.gameObject.SetActive(false);
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

    public void Sync(int health)
    {
        var i = 0;
        foreach (var icon in HealthIcons)
        {
            i++;
            icon.SetIsActive(i <= health);
        }
    }

    public void Update()
    {
        Sync(Player.Instance.Health);
    }
}
