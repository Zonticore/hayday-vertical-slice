using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class UiRewardFlyer : MonoBehaviour
{
    private const bool verbose = false;
    private const float FlightSeconds = 0.65f;

    public static void Fly(Sprite sprite, Vector3 worldPosition, RectTransform target, Action onArrived)
    {
        if (target == null)
        {
            onArrived?.Invoke();
            return;
        }

        Canvas canvas = target.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : null;
        if (canvasRect == null)
        {
            onArrived?.Invoke();
            return;
        }

        var rewardObject = new GameObject("FlyingReward", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        rewardObject.layer = canvas.gameObject.layer;
        RectTransform rect = rewardObject.GetComponent<RectTransform>();
        rect.SetParent(canvasRect, false);
        rect.SetAsLastSibling();
        rect.sizeDelta = new Vector2(64f, 64f);

        Image image = rewardObject.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        Camera worldCamera = Camera.main;
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 screenStart = worldCamera != null
            ? worldCamera.WorldToScreenPoint(worldPosition)
            : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenStart, uiCamera, out Vector2 localStart);
        rect.anchoredPosition = localStart;

        UiRewardFlyer flyer = rewardObject.AddComponent<UiRewardFlyer>();
        flyer.StartCoroutine(flyer.Animate(rect, canvasRect, target, uiCamera, onArrived));
    }

    private IEnumerator Animate(RectTransform rect, RectTransform canvasRect, RectTransform target,
        Camera uiCamera, Action onArrived)
    {
        Vector2 start = rect.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < FlightSeconds && target != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / FlightSeconds);
            float eased = 1f - Mathf.Pow(1f - progress, 3f);
            Vector2 targetScreen = RectTransformUtility.WorldToScreenPoint(uiCamera, target.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, targetScreen, uiCamera, out Vector2 end);
            Vector2 position = Vector2.LerpUnclamped(start, end, eased);
            position.y += Mathf.Sin(progress * Mathf.PI) * 90f;
            rect.anchoredPosition = position;
            rect.localScale = Vector3.one * Mathf.Lerp(1f, 0.55f, eased);
            yield return null;
        }

        onArrived?.Invoke();
        Destroy(gameObject);
        if (verbose) Log.info("[UiRewardFlyer] Reward reached its HUD target.");
    }
}
