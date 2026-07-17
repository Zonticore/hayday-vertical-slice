using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class ContextMenuDisplay : MonoBehaviour
{
    private const bool verbose = false;

    [Header("Display")]
    [Tooltip("The visual root that is shown and hidden. Keep this as a child of the object holding this component.")]
    [SerializeField] private RectTransform displayRoot;
    [SerializeField] private RectTransform optionContainer;
    [SerializeField] private ContextMenuOptionDisplay optionPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TileToolInputRouter toolInputRouter;

    [Header("Positioning")]
    [Tooltip("Pixel offset from the tile's selected screen position.")]
    [SerializeField] private Vector2 screenOffset = new Vector2(72f, 72f);
    [SerializeField] private bool keepWithinParent = true;

    private readonly List<ContextMenuOptionDisplay> _spawnedOptions =
        new List<ContextMenuOptionDisplay>();

    private bool _isSubscribed;
    private int _draggingActionIndex = -1;
    private bool _closeMenuAfterDrag;
    private Vector2 _lastTrackedScreenPosition = new Vector2(float.NaN, float.NaN);

    private void Awake()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (toolInputRouter == null)
        {
            toolInputRouter = FindAnyObjectByType<TileToolInputRouter>();
        }

        if (displayRoot != null)
        {
            displayRoot.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        Subscribe();
    }

    private void LateUpdate()
    {
        ContextMenuController controller = ContextMenuController.instance;
        Camera worldCamera = Camera.main;
        if (controller == null || !controller.IsOpen || worldCamera == null || displayRoot == null)
        {
            return;
        }

        Vector2 screenPosition = worldCamera.WorldToScreenPoint(controller.CurrentTargetWorldPosition);
        if (Vector2.SqrMagnitude(screenPosition - _lastTrackedScreenPosition) > 0.25f)
        {
            PositionNextToTile(screenPosition);
            _lastTrackedScreenPosition = screenPosition;
        }
    }

    private void OnDisable()
    {
        CancelActiveDrag();
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (_isSubscribed)
        {
            return;
        }

        ContextMenuController controller = ContextMenuController.instance;
        if (controller == null)
        {
            if (verbose) Log.warning("[ContextMenuDisplay] No ContextMenuController is available.");
            return;
        }

        controller.Opened += HandleOpened;
        controller.Closed += HandleClosed;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed)
        {
            return;
        }

        ContextMenuController controller = ContextMenuController.instance;
        if (controller != null)
        {
            controller.Opened -= HandleOpened;
            controller.Closed -= HandleClosed;
        }

        _isSubscribed = false;
    }

    private void HandleOpened(
        IReadOnlyList<ContextAction> actions,
        Vector2 tileScreenPosition)
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        ClearOptions();
        displayRoot.gameObject.SetActive(true);

        for (int i = 0; i < actions.Count; i++)
        {
            int actionIndex = i;
            ContextMenuOptionDisplay option = Instantiate(
                optionPrefab,
                optionContainer,
                false);

            option.Bind(
                actions[i],
                () => ContextMenuController.instance.Execute(actionIndex),
                screenPosition => BeginOptionDrag(actionIndex, screenPosition),
                screenPosition => ContinueOptionDrag(actionIndex, screenPosition),
                screenPosition => EndOptionDrag(actionIndex, screenPosition));
            _spawnedOptions.Add(option);
        }

        PositionNextToTile(tileScreenPosition);
        _lastTrackedScreenPosition = tileScreenPosition;

        if (verbose)
        {
            Log.info($"[ContextMenuDisplay] Displaying {actions.Count} context options.");
        }
    }

    private void HandleClosed()
    {
        CancelActiveDrag();
        ClearOptions();

        if (displayRoot != null)
        {
            displayRoot.gameObject.SetActive(false);
        }

        _lastTrackedScreenPosition = new Vector2(float.NaN, float.NaN);

        if (verbose) Log.info("[ContextMenuDisplay] Context menu closed.");
    }

    private bool BeginOptionDrag(int actionIndex, Vector2 screenPosition)
    {
        ContextMenuController menuController = ContextMenuController.instance;
        if (menuController == null || toolInputRouter == null)
        {
            if (verbose) Log.warning("[ContextMenuDisplay] Dragging requires menu and tool input controllers.");
            return false;
        }

        if (actionIndex < 0 || actionIndex >= menuController.CurrentActions.Count)
        {
            return false;
        }

        ContextAction action = menuController.CurrentActions[actionIndex];
        if (!action.IsEnabled || action.Tool == null)
        {
            return false;
        }

        CancelActiveDrag();

        if (!menuController.TryExecute(actionIndex, false))
        {
            return false;
        }

        ToolController toolController = ToolController.instance;
        if (toolController == null || !toolController.HasActiveTool)
        {
            return false;
        }

        _draggingActionIndex = actionIndex;
        _closeMenuAfterDrag = action.CloseMenuOnExecute;
        toolInputRouter.BeginStroke(screenPosition, false);
        return true;
    }

    private void ContinueOptionDrag(int actionIndex, Vector2 screenPosition)
    {
        if (actionIndex == _draggingActionIndex && toolInputRouter != null)
        {
            toolInputRouter.ContinueStroke(screenPosition);
        }
    }

    private void EndOptionDrag(int actionIndex, Vector2 screenPosition)
    {
        if (actionIndex != _draggingActionIndex)
        {
            return;
        }

        bool shouldCloseMenu = _closeMenuAfterDrag;

        if (toolInputRouter != null)
        {
            toolInputRouter.ContinueStroke(screenPosition);
            toolInputRouter.EndStroke();
        }

        ToolController.instance.Deactivate();
        ResetDragState();

        if (shouldCloseMenu)
        {
            ContextMenuController.instance.Close();
        }
    }

    private void CancelActiveDrag()
    {
        if (_draggingActionIndex < 0)
        {
            return;
        }

        toolInputRouter.EndStroke();
        ToolController.instance.Deactivate();
        ResetDragState();
    }

    private void ResetDragState()
    {
        _draggingActionIndex = -1;
        _closeMenuAfterDrag = false;
    }

    private bool HasRequiredReferences()
    {
        bool isConfigured = displayRoot != null &&
                            optionContainer != null &&
                            optionPrefab != null;

        if (!isConfigured && verbose)
        {
            Log.error("[ContextMenuDisplay] Display Root, Option Container, and Option Prefab are required.");
        }

        return isConfigured;
    }

    private void PositionNextToTile(Vector2 tileScreenPosition)
    {
        RectTransform parent = displayRoot.parent as RectTransform;
        if (parent == null)
        {
            if (verbose) Log.warning("[ContextMenuDisplay] Display Root requires a RectTransform parent.");
            return;
        }

        Camera eventCamera = GetCanvasCamera();
        Vector2 targetScreenPosition = tileScreenPosition + screenOffset;

        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parent,
                targetScreenPosition,
                eventCamera,
                out Vector3 worldPosition))
        {
            if (verbose) Log.warning("[ContextMenuDisplay] Could not convert the tile position into UI space.");
            return;
        }

        displayRoot.position = worldPosition;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(displayRoot);

        if (keepWithinParent)
        {
            ClampToParent(parent);
        }
    }

    private Camera GetCanvasCamera()
    {
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return canvas.worldCamera;
    }

    private void ClampToParent(RectTransform parent)
    {
        var parentCorners = new Vector3[4];
        var displayCorners = new Vector3[4];
        parent.GetWorldCorners(parentCorners);
        displayRoot.GetWorldCorners(displayCorners);

        Vector3 correction = Vector3.zero;

        if (displayCorners[0].x < parentCorners[0].x)
        {
            correction.x = parentCorners[0].x - displayCorners[0].x;
        }
        else if (displayCorners[2].x > parentCorners[2].x)
        {
            correction.x = parentCorners[2].x - displayCorners[2].x;
        }

        if (displayCorners[0].y < parentCorners[0].y)
        {
            correction.y = parentCorners[0].y - displayCorners[0].y;
        }
        else if (displayCorners[2].y > parentCorners[2].y)
        {
            correction.y = parentCorners[2].y - displayCorners[2].y;
        }

        displayRoot.position += correction;
    }

    private void ClearOptions()
    {
        for (int i = 0; i < _spawnedOptions.Count; i++)
        {
            if (_spawnedOptions[i] != null)
            {
                _spawnedOptions[i].gameObject.SetActive(false);
                Destroy(_spawnedOptions[i].gameObject);
            }
        }

        _spawnedOptions.Clear();
    }
}
