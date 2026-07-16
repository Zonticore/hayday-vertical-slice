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

    [Header("Positioning")]
    [Tooltip("Pixel offset from the tile's selected screen position.")]
    [SerializeField] private Vector2 screenOffset = new Vector2(72f, 72f);
    [SerializeField] private bool keepWithinParent = true;

    private readonly List<ContextMenuOptionDisplay> _spawnedOptions =
        new List<ContextMenuOptionDisplay>();

    private bool _isSubscribed;

    private void Awake()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
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

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (_isSubscribed)
        {
            return;
        }

        ContextMenuController.instance.Opened += HandleOpened;
        ContextMenuController.instance.Closed += HandleClosed;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed || ContextMenuController.instance == null)
        {
            return;
        }

        ContextMenuController.instance.Opened -= HandleOpened;
        ContextMenuController.instance.Closed -= HandleClosed;
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

            option.Bind(actions[i], () => ContextMenuController.instance.Execute(actionIndex));
            _spawnedOptions.Add(option);
        }

        PositionNextToTile(tileScreenPosition);

        if (verbose)
        {
            Log.info($"[ContextMenuDisplay] Displaying {actions.Count} context options.");
        }
    }

    private void HandleClosed()
    {
        ClearOptions();

        if (displayRoot != null)
        {
            displayRoot.gameObject.SetActive(false);
        }

        if (verbose) Log.info("[ContextMenuDisplay] Context menu closed.");
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
