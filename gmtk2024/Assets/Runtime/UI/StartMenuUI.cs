using UnityEngine;
using UnityEngine.UI;

public class StartMenuUI : MonoSingleton<StartMenuUI>
{
    public Button StartButton;
    public Button BackButton;
    public Button GuideButton;

    public RectTransform GuidePanel;

    public RectTransform InnerPanel;

    public RectTransform Background;

    public void Show()
    {
        _Starting = false;
        StartButton.gameObject.SetActive(true);
        InnerPanel.gameObject.SetActive(true);
        Background.gameObject.SetActive(true);
        BackButton.gameObject.SetActive(false);
        GuideButton.gameObject.SetActive(true);
        GuidePanel.gameObject.SetActive(false);
    }

    public void Hide()
    {
        _Starting = true;
        StartButton.gameObject.SetActive(false);
        InnerPanel.gameObject.SetActive(false);
        Background.gameObject.SetActive(false);
        BackButton.gameObject.SetActive(false);
        GuideButton.gameObject.SetActive(false);
        GuidePanel.gameObject.SetActive(false);
    }

    public void Start()
    {
        StartButton.onClick.AddListener(OnStartButtonClicked);
        GuideButton.onClick.AddListener(OnGuideButtonClicked);
        BackButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnGuideButtonClicked()
    {
        var initialPosition = new Vector3(0, Screen.height, 0);
        var targetPosition = GuidePanel.anchoredPosition;
        GuidePanel.anchoredPosition = initialPosition;
        GuidePanel.DOAnchorPos(targetPosition, 0.5f).SetEase(Ease.OutBounce);
        GuidePanel.gameObject.SetActive(true);
        BackButton.gameObject.SetActive(true);
    }

    private void OnBackButtonClicked()
    {
        var target = new Vector3(0, Screen.height, 0);
        var initialPosition = GuidePanel.anchoredPosition;
        GuidePanel
            .DOAnchorPos(target, 0.5f)
            .SetEase(Ease.InBounce)
            .OnComplete(() =>
            {
                GuidePanel.gameObject.SetActive(false);
                BackButton.gameObject.SetActive(false);
                GuidePanel.anchoredPosition = initialPosition;
            });
    }

    bool _Starting = false;

    private void OnStartButtonClicked()
    {
        if (_Starting)
            return;

        _Starting = true;

        var duration = 1.5f;
        var targetHeight = 0f;
        InnerPanel.gameObject.SetActive(false);
        Background.GetComponent<Image>().color = Color.black;
        Background.DOAnchorPosY(Background.sizeDelta.y, duration).SetEase(Ease.OutQuad);
        Background
            .DOSizeDelta(new Vector2(Background.sizeDelta.x, targetHeight), duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                Hide();
                GameManager.Instance.SetGameState(GameState.Building);
            });
    }
}
