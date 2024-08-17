using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI
    : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerUpHandler,
        IPointerDownHandler
{
    public Card Card;

    public Image Image;

    public TMPro.TMP_Text Header;

    public void SetCardData(Card card)
    {
        Card = card;
        UpdateElements();
    }

    [Button("Update Elements")]
    public void UpdateElements()
    {
        Image.sprite = Card.IconAsset;
        Header.text = Card.CardName;
    }

    void Update()
    {
        if (_IsDragging)
        {
            var distance = math.distance(_MousePos, Input.mousePosition);
            const float k_Threshold = 0;
        }
    }

    bool _IsDragging = false;
    InputState _LastState = InputState.Building;
    float3 _MousePos = 0;

    public void OnPointerDown(PointerEventData eventData)
    {
        _IsDragging = true;
        _LastState = GameManager.Instance.InputState;
        GameManager.Instance.SetInputState(InputState.Menu);
        _MousePos = Input.mousePosition;
    }

    public void OnPointerEnter(PointerEventData eventData) { }

    public void OnPointerExit(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _IsDragging = false;
        DraggingDisabled();
    }

    void DraggingDisabled()
    {
        GameManager.Instance.SetInputState(_LastState);
    }
}
