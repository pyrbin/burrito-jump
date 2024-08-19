using FMODUnity;
using TMPro;
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
    public ButtonHoverHandler DiscardHoverState;
    public Image Background;

    private RectTransform _RectTransform;
    private Vector3 _OriginalScale;
    private Vector3 _OriginalImagePosition;
    private Vector3 _OriginalTextScale;

    public bool IsUpgradeCard = false;

    public static Card? s_CardInAction;

    public static Action<float3>? s_StartedDragging;

    public static Action? s_EndedDragging;
    public static Action? s_HoverStarted;

    public static bool s_OverValidTarget = false;

    public Color SpawnColor;
    public Color MorphColor;
    public Color ActionColor;

    public void SetCardData(Card card)
    {
        Card = card;
        UpdateElements();
        SpawnAnimation();

        if (IsUpgradeCard)
        {
            Discard.gameObject.SetActive(false);
        }
    }

    [Button("Update Elements")]
    public void UpdateElements()
    {
        Image.sprite = Card.IconAsset;
        Header.text = Card.CardName;

        switch (Card.Action)
        {
            case CardAction.Spawn:
                Background.color = SpawnColor;
                break;
            case CardAction.Morph:
                Background.color = MorphColor;
                break;
            case CardAction.Action:
                Background.color = ActionColor;
                break;
        }

        var rectTransform = Image.rectTransform;
        var sprite = Card.IconAsset;
        var spriteAspectRatio = (float)sprite.texture.width / sprite.texture.height;
        rectTransform.sizeDelta = new Vector2(50 * spriteAspectRatio, 50);
    }

    void Awake()
    {
        _RectTransform = GetComponent<RectTransform>();
        _OriginalScale = _RectTransform.localScale;
        _OriginalImagePosition = Image.rectTransform.localPosition;
        _OriginalTextScale = Header.rectTransform.localScale;
        Discard.onClick.AddListener(OnDiscard);
        if (!IsUpgradeCard)
            Discard.gameObject.SetActive(true);

        DiscardHoverState = Discard.GetComponent<ButtonHoverHandler>();
    }

    void OnDiscard()
    {
        if (IsUpgradeCard)
            return;
        Player.Instance.DiscardCardFromHand(Card);
        Discard.gameObject.SetActive(false);
    }

    void Update()
    {
        if (IsUpgradeCard)
            return;
        if (_IsDragging && (Card.Action == CardAction.Spawn || Card.Action == CardAction.Morph))
        {
            var distance = Vector3.Distance(_MousePos, Input.mousePosition);
            const float k_Threshold = 220f;
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
                Player.Instance.ActivateCard(Card, block);
            }
        }

        if (_IsDragging && Card.Action == CardAction.Action)
        {
            if (s_HintOnDestroyCard)
            {
                s_HintOnDestroyCard = false;
                NotificationUI.Instance.ShowMessage(
                    "Use this card on a block by dragging your mouse over it and release.",
                    10.Secs()
                );
            }
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
        if (_IsHovering && DiscardHoverState.IsHovered && !_ResetForDiscardHover)
        {
            _ResetForDiscardHover = true;
            ResetHoverAnimation();
        }
        if (_IsHovering && !DiscardHoverState.IsHovered && _ResetForDiscardHover)
        {
            HoverAnimation();
            _ResetForDiscardHover = false;
        }
    }

    bool _ResetForDiscardHover = false;

    bool _IsDragging = false;
    InputState _LastState = InputState.Building;
    Vector3 _MousePos = Vector3.zero;
    bool _ActionsDisabled => s_CardInAction || BuildingController.Instance.IsDropping;
    public static bool s_EnableUpgrade = false;
    public static bool s_HintOnDestroyCard = true;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (DiscardHoverState.IsHovered)
            return;
        if (_DoingAnimation)
            return;
        if (IsUpgradeCard && s_EnableUpgrade)
        {
            GameManager.Instance.AddUpgradeCardToDeck(Card);
            s_EnableUpgrade = false;
        }
        if (GameManager.Instance.GameState == GameState.Upgrades)
            return;
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

    bool _IsHovering = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DiscardHoverState.IsHovered)
            return;
        if (_DoingAnimation || _ActionsDisabled)
            return;
        if (
            GameManager.Instance.GameState != GameState.Upgrades
            && BuildingController.Instance.currentBlock != null
            && Card.Action == CardAction.Spawn
        )
        {
            if (!BuildingController.Instance.IsDropping)
            {
                NotificationUI.Instance.HideMessage();
                NotificationUI.Instance.ShowMessage("A block is already active!", 2000.Ms());
                ShakeAnimation();
            }
            return;
        }
        if (
            GameManager.Instance.GameState != GameState.Upgrades
            && BuildingController.Instance.currentBlock == null
            && Card.Action == CardAction.Morph
        )
        {
            NotificationUI.Instance.HideMessage();
            NotificationUI.Instance.ShowMessage(
                "You need an active block to use this card!",
                2500.Ms()
            );
            ShakeAnimation();
            return;
        }
        if (GameManager.Instance.GameState != GameState.Upgrades && _ActionsDisabled)
            return;
        _IsHovering = true;
        _ResetForDiscardHover = false;
        HoverAnimation();
        s_HoverStarted?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_DoingAnimation)
            return;
        if (
            GameManager.Instance.GameState != GameState.Upgrades
            && BuildingController.Instance.currentBlock != null
            && Card.Action == CardAction.Spawn
        )
            return;
        if (
            GameManager.Instance.GameState != GameState.Upgrades
            && BuildingController.Instance.currentBlock == null
            && Card.Action == CardAction.Morph
        )
            return;
        if (GameManager.Instance.GameState != GameState.Upgrades && _ActionsDisabled)
            return;
        _IsHovering = false;
        if (!_ResetForDiscardHover)
            ResetHoverAnimation();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_DoingAnimation)
            return;
        if (IsUpgradeCard)
            return;
        if (GameManager.Instance.GameState == GameState.Upgrades)
            return;
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
                Player.Instance.ActivateCard(Card, block);
                NotificationUI.Instance.HideMessage();
            }
        }
        else if (
            _IsDragging && (Card.Action == CardAction.Spawn || Card.Action == CardAction.Morph)
        )
        {
            Block? block = null;
            if (Card.Action == CardAction.Morph)
                block = BuildingController.Instance.currentBlock;
            Player.Instance.ActivateCard(Card, block);
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
        if (Player.Instance.Hand.Count != 0 && GameManager.Instance.GameState == GameState.Building)
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
        _RectTransform.DOShakePosition(0.33f, 7f, 5, 90f, false, true).SetEase(Ease.OutQuad);
    }

    bool _DoingAnimation = false;

    void SpawnAnimation()
    {
        _DoingAnimation = true;
        _RectTransform
            .DOScale(_OriginalScale * 1.1f, 0.2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                _RectTransform.DOScale(_OriginalScale, 0.2f).SetEase(Ease.InQuad);
                _DoingAnimation = false;
            });
    }

    void HoverAnimation()
    {
        _RectTransform.DOScale(_OriginalScale * 1.1f, 0.2f).SetEase(Ease.OutQuad);
        Image.rectTransform.DOLocalMoveY(_OriginalImagePosition.y + 7, 0.2f).SetEase(Ease.OutQuad);
    }

    void ResetHoverAnimation()
    {
        _RectTransform.DOScale(_OriginalScale, 0.2f).SetEase(Ease.OutQuad);
        Image.rectTransform.DOLocalMoveY(_OriginalImagePosition.y, 0.2f).SetEase(Ease.OutQuad);
        Header.rectTransform.DOScale(_OriginalTextScale, 0.2f).SetEase(Ease.OutQuad);
    }
}
