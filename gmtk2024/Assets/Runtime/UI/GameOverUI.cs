using TMPro;
using UnityEngine.UI;

public class GameOverUI : MonoSingleton<GameOverUI>
{
    public TMP_Text CardsUsedText;
    public TMP_Text HeightText;
    public TMP_Text ScoreText;

    private RectTransform _RectTransform;

    private void Start()
    {
        _RectTransform = GetComponent<RectTransform>();
    }

    public void Show(int cardUsed, float height)
    {
        foreach (var t in transform.EnumerateHierarchy())
        {
            if (t == this)
                continue;
            t.gameObject.SetActive(true);
        }

        int score = CalculateScore(cardUsed, height);
        CardsUsedText.text = $"{cardUsed}";
        HeightText.text = $"{height:F2}";
        ScoreText.text = $"{score}";

        PerformFallDownAnimation();
    }

    public void Hide()
    {
        foreach (var t in transform.EnumerateHierarchy())
        {
            if (t == this)
                continue;
            t.gameObject.SetActive(false);
        }
    }

    private int CalculateScore(int cardUsed, float height)
    {
        var score = (int)Mathfs.Clamp(height - (float)cardUsed * 0.75f, 1, 9999999);
        return score;
    }

    private void PerformFallDownAnimation()
    {
        _RectTransform ??= GetComponent<RectTransform>();

        Vector3 initialPosition = new Vector3(0, Screen.height, 0);
        Vector3 targetPosition = new Vector3(0, 0, 0);
        _RectTransform.anchoredPosition = initialPosition;
        _RectTransform.DOAnchorPos(targetPosition, 0.5f).SetEase(Ease.OutBounce);
    }
}
