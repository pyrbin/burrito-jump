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
        _Active = active;
        ImageRefererence.sprite = _Active ? FilledImage : EmptyImage;
    }
}
