using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class StoreItem : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private const bool verbose = false;

    [SerializeField] private ItemDisplay image;
    [SerializeField] private TMP_Text cost;

    private StoreItemData _item;
    private StoreScreen _storeScreen;
    private Canvas _dragCanvas;
    private RectTransform _dragPreview;
    private bool _buildingPlacementHandedOff;

    public event Action<StoreItemData> Purchased;

    public void Setup(StoreItemData item)
    {
        Setup(item, GetComponentInParent<StoreScreen>());
    }

    public void Setup(StoreItemData item, StoreScreen storeScreen)
    {
        _item = item;
        _storeScreen = storeScreen;
        _buildingPlacementHandedOff = false;
        EnsureReferences();

        if (_item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.name = $"StoreItem_{_item.ItemId}";
        image.SetItem(_item.ItemId, _item.Sprite);

        if (cost != null)
        {
            cost.text = _item.Cost.ToString("N0");
        }

        gameObject.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_item == null || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        CreateDragPreview();
        MoveDragPreview(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (TryHandoffBuildingPlacement(eventData))
        {
            return;
        }

        MoveDragPreview(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DestroyDragPreview();

        if (_item == null || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (_buildingPlacementHandedOff || _item.IsBuilding)
        {
            return;
        }

        Camera worldCamera = Camera.main;
        if (worldCamera == null)
        {
            return;
        }

        Vector3 screenPoint = new Vector3(
            eventData.position.x,
            eventData.position.y,
            Mathf.Abs(worldCamera.transform.position.z));
        Vector3 worldPoint = worldCamera.ScreenToWorldPoint(screenPoint);
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);

        for (int i = 0; i < hits.Length; i++)
        {
            if (!ComponentInterfaceUtility.TryGetInterfaceInParent(
                    hits[i].gameObject,
                    out IStoreItemDropTarget target) ||
                !target.CanAccept(_item))
            {
                continue;
            }

            if (target.TryAccept(_item))
            {
                Purchased?.Invoke(_item);
            }

            return;
        }

        if (verbose)
        {
            Log.info($"[StoreItem] No valid drop target for '{_item.ItemId}'.");
        }
    }

    private bool TryHandoffBuildingPlacement(PointerEventData eventData)
    {
        if (_item == null ||
            !_item.IsBuilding ||
            _buildingPlacementHandedOff ||
            eventData.button != PointerEventData.InputButton.Left ||
            _storeScreen == null ||
            _storeScreen.ContainsScreenPoint(eventData.position))
        {
            return false;
        }

        BuildingPlacementController controller = BuildingPlacementController.GetOrCreate();
        if (controller == null || !controller.BeginPlacement(_item, eventData.position))
        {
            return false;
        }

        _buildingPlacementHandedOff = true;
        DestroyDragPreview();
        _storeScreen.CloseForBuildingPlacement();
        return true;
    }

    private void EnsureReferences()
    {
        if (image == null)
        {
            image = GetComponentInChildren<ItemDisplay>(true);
        }

        if (image == null)
        {
            image = gameObject.AddComponent<ItemDisplay>();
        }

        if (cost == null)
        {
            cost = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void CreateDragPreview()
    {
        Canvas nearestCanvas = GetComponentInParent<Canvas>();
        _dragCanvas = nearestCanvas != null ? nearestCanvas.rootCanvas : null;
        if (_dragCanvas == null)
        {
            return;
        }

        var previewObject = new GameObject(
            $"StoreDrag_{_item.ItemId}",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(CanvasGroup),
            typeof(Image));

        _dragPreview = previewObject.GetComponent<RectTransform>();
        _dragPreview.SetParent(_dragCanvas.transform, false);
        _dragPreview.SetAsLastSibling();
        _dragPreview.sizeDelta = new Vector2(128f, 128f);

        Image previewImage = previewObject.GetComponent<Image>();
        previewImage.sprite = _item.Sprite;
        previewImage.preserveAspect = true;
        previewImage.raycastTarget = false;

        CanvasGroup group = previewObject.GetComponent<CanvasGroup>();
        group.alpha = 0.9f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    private void MoveDragPreview(Vector2 screenPosition)
    {
        if (_dragPreview == null || _dragCanvas == null)
        {
            return;
        }

        RectTransform canvasRect = (RectTransform)_dragCanvas.transform;
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
        DestroyDragPreview();
    }
}
