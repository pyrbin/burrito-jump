using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHolderUI : MonoBehaviour
{
    public float SpaceBetween = 10f;
    public GameObject CardUIPrefab;
    public List<CardUI> SpawnedCards = new();
    public TMP_Text CardText;
    public CardDragIndicator CardDragIndicator;
    public Button StartButton;

    public void Show()
    {
        foreach (var t in transform.EnumerateHierarchy())
        {
            if (t == this)
                continue;
            t.gameObject.SetActive(true);
        }
        CardDragIndicator.gameObject.SetActive(true);
        if (SpawnedCards.Count > 0 && GameManager.Instance.GameState == GameState.Building)
        {
            StartButton.gameObject.SetActive(true);
        }
        else
        {
            StartButton.gameObject.SetActive(false);
        }
        CardText.gameObject.SetActive(true);
        RetainOrder();
    }

    public void Hide()
    {
        foreach (var t in transform.EnumerateHierarchy())
        {
            if (t == this)
                continue;
            t.gameObject.SetActive(false);
        }
        CardDragIndicator.gameObject.SetActive(false);
        StartButton.gameObject.SetActive(false);
        CardText.gameObject.SetActive(false);
    }

    public void Awake()
    {
        StartButton.onClick.AddListener(() =>
        {
            GameManager.Instance.SetGameState(GameState.Platforming);
        });
        Hide();
    }

    public void Update()
    {
        RetainOrder();

        if (SpawnedCards.Count > 0 && GameManager.Instance.GameState == GameState.Building)
        {
            StartButton.gameObject.SetActive(true);
        }
        else
        {
            StartButton.gameObject.SetActive(false);
        }
    }

    public void Reset()
    {
        foreach (var cardUI in SpawnedCards.ToList())
        {
            RemoveCard(cardUI.Card);
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

        CardText.text = Player.Instance.ActiveDeck.Count.ToString();
    }

    public void AddCard(Card card)
    {
        var cardUIObject = Instantiate(CardUIPrefab, parent: this.transform);
        var cardUI = cardUIObject.GetComponent<CardUI>();
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
            cardToRemove
                .transform.DOScale(Vector3.zero, 0.444f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    SpawnedCards.Remove(cardToRemove);
                    Destroy(cardToRemove.gameObject);
                    RetainOrder();
                });
        }
    }

    [Button("Retain")]
    public void RetainOrder()
    {
        if (SpawnedCards.Count == 0)
            return;

        var totalHeight = 0f;
        var copy = SpawnedCards.ToList();
        copy.Reverse();
        foreach (var cardUI in copy)
        {
            totalHeight += cardUI.GetComponent<RectTransform>().sizeDelta.y;
        }
        totalHeight += (SpawnedCards.Count - 1) * SpaceBetween;

        var startY = totalHeight / 2f;

        for (var i = 0; i < copy.Count; i++)
        {
            var rectTransform = copy[i].GetComponent<RectTransform>();
            var cardHeight = rectTransform.sizeDelta.y;
            var cardPositionY = -startY + (i * (cardHeight + SpaceBetween));
            rectTransform.anchoredPosition = new Vector2(0, cardPositionY + cardHeight / 2f);
        }
    }
}
