using UnityEngine.UI;

public class HealthIconUI : MonoBehaviour
{
    public Sprite FilledImage;
    public Sprite EmptyImage;
    public Image ImageRefererence;

    private bool _Active = true;
    public bool IsActive => _Active;

    public void SetIsActive(bool active)
    {
        if (_Active == active)
            return;

        _Active = active;
        ImageRefererence.sprite = FilledImage;
        ImageRefererence.gameObject.SetActive(active);
    }
}
