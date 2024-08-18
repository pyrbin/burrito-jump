using UnityEngine;
using UnityEngine.UI;

public class StartMenuUI : MonoSingleton<StartMenuUI>
{
    public Button StartButton;

    public RectTransform InnerPanel;

    public RectTransform Background;

    public void Show()
    {
        _Starting = false;
        StartButton.gameObject.SetActive(true);
        InnerPanel.gameObject.SetActive(true);
        Background.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _Starting = true;
        StartButton.gameObject.SetActive(false);
        InnerPanel.gameObject.SetActive(false);
        Background.gameObject.SetActive(false);
    }

    public void Start()
    {
        StartButton.onClick.AddListener(OnStartButtonClicked);
    }

    bool _Starting = false;

    private void OnStartButtonClicked()
    {
        if (_Starting)
            return;

        _Starting = true;

        var duration = 0.5f;
        var targetHeight = 0f;
        InnerPanel.gameObject.SetActive(false);
        Background.DOAnchorPosY(Background.sizeDelta.y, duration).SetEase(Ease.InQuad);
        Background
            .DOSizeDelta(new Vector2(Background.sizeDelta.x, targetHeight), duration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                Hide();
                GameManager.Instance.SetGameState(GameState.Building);
            });
    }
}
