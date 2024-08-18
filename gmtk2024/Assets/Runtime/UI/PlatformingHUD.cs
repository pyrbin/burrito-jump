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

    public void Sync(int health)
    {
        // if anyone sees this, i'm sorry :P
        if (health == 0)
        {
            Health1.SetIsActive(false);
            Health2.SetIsActive(false);
            Health3.SetIsActive(false);
        }

        if (health == 1)
        {
            Health1.SetIsActive(true);
            Health2.SetIsActive(false);
            Health3.SetIsActive(false);
        }

        if (health == 2)
        {
            Health1.SetIsActive(true);
            Health2.SetIsActive(true);
            Health3.SetIsActive(false);
        }

        if (health == 3)
        {
            Health1.SetIsActive(true);
            Health2.SetIsActive(true);
            Health3.SetIsActive(true);
        }
    }

    public void Update()
    {
        Sync(Player.Instance.Health);
    }
}
