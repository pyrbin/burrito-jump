using DG.Tweening;
using TMPro;
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
    public TMP_Text Header;
    public Button Discard;

    private RectTransform _RectTransform;
    private Vector3 _OriginalScale;
    private Vector3 _OriginalImagePosition;
    private Vector3 _OriginalTextScale;

    public static Card? s_CardInAction;

    public static Action<float3>? s_StartedDragging;

    public static Action? s_EndedDragging;

    public static bool s_OverValidTarget = false;

    public void SetCardData(Card card)
    {
        Card = card;
        UpdateElements();
        SpawnAnimation();
    }

    [Button("Update Elements")]
    public void UpdateElements()
    {
        Image.sprite = Card.IconAsset;
        Header.text = Card.CardName;
    }

    void Awake()
    {
        _RectTransform = GetComponent<RectTransform>();
        _OriginalScale = _RectTransform.localScale;
        _OriginalImagePosition = Image.rectTransform.localPosition;
        _OriginalTextScale = Header.rectTransform.localScale;
        Discard.onClick.AddListener(OnDiscard);
        Discard.gameObject.SetActive(true);
    }

    void OnDiscard()
    {
        Player.Instance.DiscardCardFromHand(Card);
        Discard.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_IsDragging && (Card.Action == CardAction.Spawn || Card.Action == CardAction.Morph))
        {
            var distance = Vector3.Distance(_MousePos, Input.mousePosition);
            const float k_Threshold = 240f;
            if (distance >= k_Threshold)
            {
                _IsDragging = false;
                DraggingDisabled();
                ResetCardSize();
                s_CardInAction = null;
                Block? block = null;
                s_EndedDragging?.Invoke();
                s_OverValidTarget = false;

                if (Card.Action == CardAction.Morph)
                    block = BuildingController.Instance.currentBlock;
                ShakeAnimation();
                Player.Instance.ActivateCard(Card, block);
            }
        }

        if (_IsDragging && Card.Action == CardAction.Action)
        {
            var mousePosition = new float3(Input.mousePosition.x, Input.mousePosition.y, 0f);
            var ray = Camera.main.ScreenPointToRay(mousePosition);
            var hit = Physics2D.GetRayIntersection(ray, 30f, LayerMask.GetMask("Block"));
            if (hit.collider is not null)
            {
                s_OverValidTarget = true;
            }
            else
            {
                s_OverValidTarget = false;
            }
        }
    }

    bool _IsDragging = false;
    InputState _LastState = InputState.Building;
    Vector3 _MousePos = Vector3.zero;
    bool _ActionsDisabled => s_CardInAction || BuildingController.Instance.IsDropping;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (BuildingController.Instance.currentBlock != null && Card.Action == CardAction.Spawn)
            return;
        if (BuildingController.Instance.currentBlock == null && Card.Action == CardAction.Morph)
            return;
        if (_ActionsDisabled)
            return;
        _IsDragging = true;
        _LastState = GameManager.Instance.InputState;
        GameManager.Instance.SetInputState(InputState.Menu);
        _MousePos = Input.mousePosition;
        s_CardInAction = Card;
        s_StartedDragging?.Invoke(_MousePos);
        s_OverValidTarget = false;
        ShrinkCard();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (BuildingController.Instance.currentBlock != null && Card.Action == CardAction.Spawn)
        {
            NotificationUI.Instance.ShowMessage("A block is already active!", 1500.Ms());
            ShakeAnimation();
            return;
        }
        if (BuildingController.Instance.currentBlock == null && Card.Action == CardAction.Morph)
        {
            NotificationUI.Instance.ShowMessage(
                "You need an active block to use this card!",
                1500.Ms()
            );
            ShakeAnimation();
            return;
        }
        if (_ActionsDisabled)
            return;
        HoverAnimation();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (BuildingController.Instance.currentBlock != null && Card.Action == CardAction.Spawn)
            return;
        if (BuildingController.Instance.currentBlock == null && Card.Action == CardAction.Morph)
            return;
        if (_ActionsDisabled)
            return;
        ResetHoverAnimation();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (BuildingController.Instance.currentBlock != null && Card.Action == CardAction.Spawn)
            return;
        if (BuildingController.Instance.currentBlock == null && Card.Action == CardAction.Morph)
            return;
        if (s_CardInAction != Card)
            return;
        if (!_IsDragging)
            return;

        if (Card.Action == CardAction.Action)
        {
            var mousePosition = new float3(Input.mousePosition.x, Input.mousePosition.y, 0f);
            var ray = Camera.main.ScreenPointToRay(mousePosition);
            var hit = Physics2D.GetRayIntersection(ray, 30f, LayerMask.GetMask("Block"));
            if (hit.collider is not null)
            {
                var block = hit.collider.gameObject.GetComponent<Block>();
                ShakeAnimation();
                Player.Instance.ActivateCard(Card, block);
            }
        }

        _IsDragging = false;
        DraggingDisabled();
        ResetCardSize();
        s_CardInAction = null;
        s_EndedDragging?.Invoke();
        s_OverValidTarget = false;
    }

    void DraggingDisabled()
    {
        GameManager.Instance.SetInputState(_LastState);
    }

    void ShrinkCard()
    {
        _RectTransform.DOScale(_OriginalScale * 0.9f, 0.2f).SetEase(Ease.OutQuad);
    }

    void ResetCardSize()
    {
        _RectTransform.DOScale(_OriginalScale, 0.2f).SetEase(Ease.OutQuad);
    }

    void ShakeAnimation()
    {
        _RectTransform.DOShakePosition(0.5f, 10f, 10, 90f, false, true).SetEase(Ease.OutQuad);
    }

    void SpawnAnimation()
    {
        _RectTransform
            .DOScale(_OriginalScale * 1.1f, 0.2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                _RectTransform.DOScale(_OriginalScale, 0.2f).SetEase(Ease.InQuad);
            });
    }

    void HoverAnimation()
    {
        _RectTransform.DOScale(_OriginalScale * 1.1f, 0.2f).SetEase(Ease.OutQuad);
        Image.rectTransform.DOLocalMoveY(_OriginalImagePosition.y + 10, 0.2f).SetEase(Ease.OutQuad);
    }

    void ResetHoverAnimation()
    {
        _RectTransform.DOScale(_OriginalScale, 0.2f).SetEase(Ease.OutQuad);
        Image.rectTransform.DOLocalMoveY(_OriginalImagePosition.y, 0.2f).SetEase(Ease.OutQuad);
        Header.rectTransform.DOScale(_OriginalTextScale, 0.2f).SetEase(Ease.OutQuad);
    }
}
