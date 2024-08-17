using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHolderUI : MonoBehaviour
{
    public float SpaceBetween = 20f;
    public GameObject CardUIPrefab; // Assign this in the Unity Editor

    public List<CardUI> SpawnedCards = new();

    public void AddCard(Card card)
    {
        var cardUIObject = Instantiate(CardUIPrefab);
        var cardUI = cardUIObject.GetComponent<CardUI>();
        cardUI.SetCardData(card);
        SpawnedCards.Add(cardUI);

        RetainOrder();
    }

    public void Removecard(Card card)
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

        // Calculate the total width of the cards including the space between them
        float totalWidth = 0f;
        foreach (var cardUI in SpawnedCards)
        {
            totalWidth += cardUI.GetComponent<RectTransform>().sizeDelta.x;
        }
        totalWidth += (SpawnedCards.Count - 1) * SpaceBetween;

        // Calculate the starting position for the first card to center the list
        float startX = -totalWidth / 2f;

        // Position each card in the list
        for (int i = 0; i < SpawnedCards.Count; i++)
        {
            RectTransform rectTransform = SpawnedCards[i].GetComponent<RectTransform>();
            float cardWidth = rectTransform.sizeDelta.x;
            float cardPositionX = startX + (i * (cardWidth + SpaceBetween));
            rectTransform.anchoredPosition = new Vector2(cardPositionX + cardWidth / 2f, 0);
        }
    }
}
