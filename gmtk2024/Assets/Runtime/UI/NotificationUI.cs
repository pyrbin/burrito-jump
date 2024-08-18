using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationUI : MonoSingleton<NotificationUI>
{
    public TMP_Text Text;
    public Image Image;

    private RectTransform _RectTransform;
    private Vector3 _OriginalPosition;
    private Vector3 _HiddenPosition;
    private bool _IsShowingMessage = false;
    private Coroutine? _CurrentCoroutine;

    public void Start()
    {
        _RectTransform = GetComponent<RectTransform>();
        _OriginalPosition = _RectTransform.localPosition;
        _HiddenPosition = _OriginalPosition + new Vector3(0, -70, 0);
        Hide();
    }

    public void ShowMessage(string message, Duration duration)
    {
        if (_IsShowingMessage)
        {
            if (_CurrentCoroutine != null)
            {
                StopCoroutine(_CurrentCoroutine);
            }
        }

        _IsShowingMessage = true;
        Text.text = message;
        Image.enabled = true;
        Text.enabled = true;

        _RectTransform.localPosition = _HiddenPosition;
        _RectTransform.DOLocalMoveY(_OriginalPosition.y, 0.5f).SetEase(Ease.OutQuad);

        _CurrentCoroutine = StartCoroutine(HideAfterDuration(duration.Seconds));
    }

    public void HideMessage()
    {
        if (_IsShowingMessage)
        {
            if (_CurrentCoroutine != null)
            {
                StopCoroutine(_CurrentCoroutine);
            }
        }

        Hide();
    }

    private IEnumerator HideAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        _RectTransform
            .DOLocalMoveY(_HiddenPosition.y, 0.5f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                Hide();
                _IsShowingMessage = false;
            });
    }

    private void Hide()
    {
        Image.enabled = false;
        Text.enabled = false;
        _RectTransform.localPosition = _HiddenPosition;
    }
}
