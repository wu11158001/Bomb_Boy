using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class ScrollbarVisibility : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float fadeDuration = 0.2f;

    private CanvasGroup _canvasGroup;
    private bool _isDown;

    void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        HideScrollbar();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowScrollbar();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isDown) return;
        HideScrollbar();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isDown = true;
        _canvasGroup.alpha = 1;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(_isDown)
        {
            _isDown = false;
            HideScrollbar();
        }
    }

    /// <summary>
    /// 顯示滑條
    /// </summary>
    private void ShowScrollbar()
    {
        StartCoroutine(FadeScrollbar(1f));
    }

    /// <summary>
    /// 隱藏滑條
    /// </summary>
    private void HideScrollbar()
    {
        StartCoroutine(FadeScrollbar(0f));
    }

    /// <summary>
    /// 滑條淡入淡出效果
    /// </summary>
    /// <param name="targetAlpha"></param>
    /// <returns></returns>
    private IEnumerator FadeScrollbar(float targetAlpha)
    {
        if (targetAlpha == 0)
        {
            yield return new WaitForSeconds(1.0f);
        }

        float startAlpha = _canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = targetAlpha;
    }
}
