using UnityEngine.EventSystems;

public class ButtonHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [ReadOnly]
    public bool IsHovered = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovered = false;
    }
}
