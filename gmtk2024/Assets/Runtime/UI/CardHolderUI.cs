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
            SpawnedCards.Remove(cardToRemove);
            Destroy(cardToRemove.gameObject);
            RetainOrder();
        }
    }

    [Button("Retain")]
    public void RetainOrder()
    {
        if (SpawnedCards.Count == 0)
            return;

        var totalHeight = 0f;
        foreach (var cardUI in SpawnedCards)
        {
            totalHeight += cardUI.GetComponent<RectTransform>().sizeDelta.y;
        }
        totalHeight += (SpawnedCards.Count - 1) * SpaceBetween;

        var startY = totalHeight / 2f;

        for (var i = 0; i < SpawnedCards.Count; i++)
        {
            var rectTransform = SpawnedCards[i].GetComponent<RectTransform>();
            var cardHeight = rectTransform.sizeDelta.y;
            var cardPositionY = -startY + (i * (cardHeight + SpaceBetween));
            rectTransform.anchoredPosition = new Vector2(0, cardPositionY + cardHeight / 2f);
        }
    }
}
