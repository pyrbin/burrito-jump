using UnityEngine;

public class CardUpgradesUI : MonoSingleton<CardUpgradesUI>
{
    public float SpaceBetween = 10f;
    public GameObject CardUIPrefab;
    public List<CardUI> SpawnedCards = new();

    public void Start()
    {
        GameManager.Instance.OnUpgradeComplete += () =>
        {
            Sync(new List<Card>());
        };

        Hide();
    }

    public void Show()
    {
        foreach (var t in transform.EnumerateHierarchy())
        {
            if (t == this)
                continue;
            t.gameObject.SetActive(true);
        }
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

    public void Sync(List<Card> cards)
    {
        var currentCards = new HashSet<Card>(SpawnedCards.Select(c => c.Card));
        var newCards = new HashSet<Card>(cards);
        foreach (var cardUI in SpawnedCards.ToList())
        {
            if (!newCards.Contains(cardUI.Card))
            {
                RemoveCard(cardUI.Card);
            }
        }
        foreach (var card in cards)
        {
            if (!currentCards.Contains(card))
            {
                AddCard(card);
            }
        }
        RetainOrder();
    }

    public void AddCard(Card card)
    {
        var cardUIObject = Instantiate(CardUIPrefab, parent: this.transform);
        var cardUI = cardUIObject.GetComponent<CardUI>();
        cardUI.IsUpgradeCard = true;
        cardUI.SetCardData(card);
        SpawnedCards.Add(cardUI);
        RetainOrder();
    }

    public void RemoveCard(Card card)
    {
        var cardToRemove = SpawnedCards.Find(c => c.Card == card);
        if (cardToRemove != null)
        {
            cardToRemove.enabled = false;
            var sequence = DOTween.Sequence();
            sequence.Append(
                cardToRemove.transform.DOShakePosition(0.255f, 10f, 10, 90f, false, true)
            );
            sequence.Join(
                cardToRemove.transform.DOScale(Vector3.zero, 0.255f).SetEase(Ease.InQuad)
            );

            sequence.OnComplete(() =>
            {
                SpawnedCards.Remove(cardToRemove);
                Destroy(cardToRemove.gameObject);
                RetainOrder();
            });
        }
    }

    public void Update()
    {
        RetainOrder();
    }

    [Button("Retain")]
    public void RetainOrder()
    {
        if (SpawnedCards.Count == 0)
            return;

        var totalWidth = 0f;
        foreach (var cardUI in SpawnedCards)
        {
            totalWidth += cardUI.GetComponent<RectTransform>().sizeDelta.x;
        }
        totalWidth += (SpawnedCards.Count - 1) * SpaceBetween;

        var startX = -totalWidth / 2f;

        for (var i = 0; i < SpawnedCards.Count; i++)
        {
            var rectTransform = SpawnedCards[i].GetComponent<RectTransform>();
            var cardWidth = rectTransform.sizeDelta.x;
            var cardPositionX = startX + (i * (cardWidth + SpaceBetween));
            rectTransform.anchoredPosition = new Vector2(cardPositionX + cardWidth / 2f, 0);
        }
    }
}
