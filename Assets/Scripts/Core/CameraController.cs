using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public sealed class CameraController : MonoBehaviourSingleton<CameraController>
{
    private const bool verbose = false;

    [Header("Panning")]
    [SerializeField, Min(0.01f)] private float dragMultiplier = 1f;
    [SerializeField] private Vector2 horizontalBounds = new Vector2(-110f, 110f);
    [SerializeField] private Vector2 verticalBounds = new Vector2(-60f, 60f);

    [Header("Zoom")]
    [SerializeField, Min(0.1f)] private float minimumZoom = 3f;
    [SerializeField, Min(0.1f)] private float maximumZoom = 14f;
    [SerializeField, Min(0.001f)] private float pinchZoomSpeed = 0.01f;
    [SerializeField, Min(0.01f)] private float scrollZoomSpeed = 0.6f;

    [Header("Context Focus")]
    [SerializeField, Min(0.01f)] private float focusSmoothTime = 0.18f;

    private Camera _camera;
    private bool _isDragging;
    private Vector2 _previousPointerPosition;
    private Vector3 _focusVelocity;

    protected override void Awake()
    {
        base.Awake();
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (IsUiOpen())
        {
            _isDragging = false;
            FocusContextTarget();
            return;
        }

        if (HandleZoom()) return;
        HandlePan();
    }

    private void HandlePan()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null) return;

        Vector2 screenPosition = pointer.position.ReadValue();
        if (pointer.press.wasPressedThisFrame)
        {
            _isDragging = !IsPointerOverCanvasUi(screenPosition);
            _previousPointerPosition = screenPosition;
            return;
        }

        if (pointer.press.wasReleasedThisFrame)
        {
            _isDragging = false;
            return;
        }

        if (!_isDragging || !pointer.press.isPressed) return;

        Vector3 previousWorld = ScreenToWorld(_previousPointerPosition);
        Vector3 currentWorld = ScreenToWorld(screenPosition);
        Vector3 movement = (previousWorld - currentWorld) * dragMultiplier;
        MoveAndClamp(transform.position + movement);
        _previousPointerPosition = screenPosition;
    }

    private bool HandleZoom()
    {
        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            if (touches.Count >= 2 && touches[0].press.isPressed && touches[1].press.isPressed)
            {
                Vector2 first = touches[0].position.ReadValue();
                Vector2 second = touches[1].position.ReadValue();
                Vector2 previousFirst = first - touches[0].delta.ReadValue();
                Vector2 previousSecond = second - touches[1].delta.ReadValue();
                float pinchDelta = Vector2.Distance(first, second) -
                                   Vector2.Distance(previousFirst, previousSecond);
                SetZoom(_camera.orthographicSize - pinchDelta * pinchZoomSpeed);
                _isDragging = false;
                return true;
            }
        }

        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (!Mathf.Approximately(scroll, 0f) &&
                !IsPointerOverCanvasUi(Mouse.current.position.ReadValue()))
            {
                SetZoom(_camera.orthographicSize - Mathf.Sign(scroll) * scrollZoomSpeed);
            }
        }

        return false;
    }

    private void FocusContextTarget()
    {
        ContextMenuController contextMenu = ContextMenuController.instance;
        if (contextMenu == null || !contextMenu.IsOpen) return;

        Vector3 current = transform.position;
        Vector3 target = contextMenu.CurrentTargetWorldPosition;
        target.z = current.z;
        Vector3 focused = Vector3.SmoothDamp(current, target, ref _focusVelocity, focusSmoothTime);
        MoveAndClamp(focused);
    }

    private bool IsUiOpen()
    {
        if (BuildingPlacementController.hasInstance &&
            BuildingPlacementController.instance.IsPlacing)
        {
            return true;
        }

        UISystem ui = UISystem.instance;
        if (ui != null) return ui.IsAnyUiOpen;
        ContextMenuController menu = ContextMenuController.instance;
        return menu != null && menu.IsOpen;
    }

    private bool IsPointerOverCanvasUi(Vector2 screenPosition)
    {
        UISystem ui = UISystem.instance;
        return ui != null && ui.IsPointerOverUi(screenPosition);
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        Vector3 point = new Vector3(screenPosition.x, screenPosition.y, -transform.position.z);
        return _camera.ScreenToWorldPoint(point);
    }

    private void SetZoom(float zoom)
    {
        _camera.orthographicSize = Mathf.Clamp(zoom, minimumZoom, maximumZoom);
        MoveAndClamp(transform.position);
    }

    private void MoveAndClamp(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, horizontalBounds.x, horizontalBounds.y);
        position.y = Mathf.Clamp(position.y, verticalBounds.x, verticalBounds.y);
        transform.position = position;
    }

    private void OnValidate()
    {
        minimumZoom = Mathf.Max(0.1f, minimumZoom);
        maximumZoom = Mathf.Max(minimumZoom, maximumZoom);
        if (horizontalBounds.x > horizontalBounds.y) horizontalBounds = new Vector2(horizontalBounds.y, horizontalBounds.x);
        if (verticalBounds.x > verticalBounds.y) verticalBounds = new Vector2(verticalBounds.y, verticalBounds.x);
        if (verbose) Log.info("[CameraController] Camera settings validated.");
    }
}
