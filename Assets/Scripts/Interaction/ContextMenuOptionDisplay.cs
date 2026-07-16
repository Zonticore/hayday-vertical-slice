using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ContextMenuOptionDisplay : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private const bool verbose = false;

    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text displayNameText;

    [Header("Drag Preview")]
    [SerializeField, Min(0.1f)] private float dragPreviewScale = 1f;
    [SerializeField, Range(0f, 1f)] private float dragPreviewAlpha = 0.9f;

    private Action _onSelected;
    private Func<Vector2, bool> _onDragStarted;
    private Action<Vector2> _onDragged;
    private Action<Vector2> _onDragEnded;
    private bool _isEnabled;
    private bool _canDrag;
    private bool _isDragging;
    private Canvas _dragCanvas;
    private RectTransform _dragPreview;

    public void Bind(
        ContextAction action,
        Action onSelected,
        Func<Vector2, bool> onDragStarted,
        Action<Vector2> onDragged,
        Action<Vector2> onDragEnded)
    {
        if (action == null)
        {
            if (verbose) Log.warning("[ContextMenuOptionDisplay] Cannot bind a null action.");
            gameObject.SetActive(false);
            return;
        }

        _onSelected = onSelected;
        _onDragStarted = onDragStarted;
        _onDragged = onDragged;
        _onDragEnded = onDragEnded;
        _isEnabled = action.IsEnabled;
        _canDrag = action.IsEnabled && action.Tool != null;
        gameObject.name = $"ContextOption_{action.ActionId}";

        if (displayNameText != null)
        {
            displayNameText.text = action.DisplayName;
        }

        if (iconImage != null)
        {
            iconImage.sprite = action.Icon;
            iconImage.enabled = action.Icon != null;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            TryBeginToolDrag(eventData.position);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            EndToolDrag(eventData.position);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left &&
            _isEnabled &&
            !_canDrag)
        {
            _onSelected?.Invoke();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        TryBeginToolDrag(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging)
        {
            return;
        }

        MoveDragPreview(eventData.position);
        _onDragged?.Invoke(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            EndToolDrag(eventData.position);
        }
    }

    private void TryBeginToolDrag(Vector2 screenPosition)
    {
        if (!_canDrag || _isDragging || _onDragStarted == null)
        {
            return;
        }

        if (!_onDragStarted.Invoke(screenPosition))
        {
            return;
        }

        _isDragging = true;
        CreateDragPreview();
        MoveDragPreview(screenPosition);
    }

    private void EndToolDrag(Vector2 screenPosition)
    {
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;
        DestroyDragPreview();
        _onDragEnded?.Invoke(screenPosition);
    }

    private void CreateDragPreview()
    {
        Canvas nearestCanvas = GetComponentInParent<Canvas>();
        _dragCanvas = nearestCanvas != null ? nearestCanvas.rootCanvas : null;
        if (_dragCanvas == null)
        {
            if (verbose) Log.warning("[ContextMenuOptionDisplay] A Canvas is required for the drag preview.");
            return;
        }

        var previewObject = new GameObject(
            $"DragPreview_{gameObject.name}",
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(Image));

        _dragPreview = previewObject.GetComponent<RectTransform>();
        _dragPreview.SetParent(_dragCanvas.transform, false);
        _dragPreview.SetAsLastSibling();

        Vector2 sourceSize = iconImage != null
            ? iconImage.rectTransform.rect.size
            : ((RectTransform)transform).rect.size;
        _dragPreview.sizeDelta = sourceSize * dragPreviewScale;

        Image previewImage = previewObject.GetComponent<Image>();
        previewImage.sprite = iconImage != null ? iconImage.sprite : null;
        previewImage.color = iconImage != null ? iconImage.color : Color.white;
        previewImage.preserveAspect = true;
        previewImage.raycastTarget = false;

        CanvasGroup canvasGroup = previewObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = dragPreviewAlpha;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void MoveDragPreview(Vector2 screenPosition)
    {
        if (_dragPreview == null || _dragCanvas == null)
        {
            return;
        }

        RectTransform canvasRect = _dragCanvas.transform as RectTransform;
        Camera eventCamera = _dragCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _dragCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                eventCamera,
                out Vector2 localPosition))
        {
            _dragPreview.localPosition = localPosition;
        }
    }

    private void DestroyDragPreview()
    {
        if (_dragPreview != null)
        {
            Destroy(_dragPreview.gameObject);
        }

        _dragPreview = null;
        _dragCanvas = null;
    }

    private void OnDisable()
    {
        _isDragging = false;
        DestroyDragPreview();
    }

    private void OnDestroy()
    {
        _onSelected = null;
        _onDragStarted = null;
        _onDragged = null;
        _onDragEnded = null;
    }

}
