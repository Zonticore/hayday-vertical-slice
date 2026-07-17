using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class UISystem : MonoBehaviourSingleton<UISystem>
{
    private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

    private string _outsideClosedScreenId = string.Empty;
    private int _outsideClosedFrame = -1;

    public List<UIScreen> allScreens = new List<UIScreen>();
    public List<UIScreen> openScreens = new List<UIScreen>();

    public bool IsContextMenuOpen => ContextMenuController.instance != null && ContextMenuController.instance.IsOpen;
    public bool IsAnyUiOpen => openScreens.Count > 0 || IsContextMenuOpen;

    protected override void Awake()
    {
        base.Awake();

        UIScreen[] discoveredScreens = FindObjectsByType<UIScreen>(
            FindObjectsInactive.Include);
        for (int i = 0; i < discoveredScreens.Length; i++)
        {
            if (discoveredScreens[i] != null && !allScreens.Contains(discoveredScreens[i]))
            {
                allScreens.Add(discoveredScreens[i]);
            }
        }

        foreach (var screen in allScreens)
        {
            if (screen != null)
            {
                screen.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        Pointer pointer = Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            Vector2 screenPosition = pointer.position.ReadValue();
            CloseTopmostScreenWhenPointerIsOutside(screenPosition);

            if (IsContextMenuOpen && !IsPointerOverContextOption(screenPosition))
            {
                ContextMenuController.instance.Close();
            }
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseTopmost();
        }
    }

    public bool IsPointerOverUi(Vector2 screenPosition)
    {
        Raycast(screenPosition);
        for (int i = 0; i < _raycastResults.Count; i++)
        {
            GameObject target = _raycastResults[i].gameObject;
            if (target != null && target.GetComponentInParent<Canvas>() != null) return true;
        }
        return false;
    }

    private bool IsPointerOverContextOption(Vector2 screenPosition)
    {
        Raycast(screenPosition);
        for (int i = 0; i < _raycastResults.Count; i++)
        {
            GameObject target = _raycastResults[i].gameObject;
            if (target != null && target.GetComponentInParent<ContextMenuOptionDisplay>() != null) return true;
        }
        return false;
    }

    private void CloseTopmostScreenWhenPointerIsOutside(Vector2 screenPosition)
    {
        UIScreen screen = GetTopmostOpenScreen();
        if (screen == null || IsPointerInsideScreen(screen, screenPosition))
        {
            return;
        }

        _outsideClosedScreenId = screen.screenId;
        _outsideClosedFrame = Time.frameCount;
        hideScreen(screen);
    }

    private UIScreen GetTopmostOpenScreen()
    {
        for (int i = openScreens.Count - 1; i >= 0; i--)
        {
            UIScreen screen = openScreens[i];
            if (screen != null && screen.gameObject.activeInHierarchy)
            {
                return screen;
            }

            openScreens.RemoveAt(i);
        }

        return null;
    }

    private bool IsPointerInsideScreen(UIScreen screen, Vector2 screenPosition)
    {
        RectTransform screenRect = screen.transform as RectTransform;
        Canvas canvas = screen.GetComponentInParent<Canvas>();
        Camera eventCamera = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        if (screenRect != null && RectTransformUtility.RectangleContainsScreenPoint(
                screenRect,
                screenPosition,
                eventCamera))
        {
            return true;
        }

        Raycast(screenPosition);
        for (int i = 0; i < _raycastResults.Count; i++)
        {
            GameObject target = _raycastResults[i].gameObject;
            if (target != null && target.transform.IsChildOf(screen.transform))
            {
                return true;
            }
        }

        return false;
    }

    private void Raycast(Vector2 screenPosition)
    {
        _raycastResults.Clear();
        if (EventSystem.current == null) return;
        var eventData = new PointerEventData(EventSystem.current) { position = screenPosition };
        EventSystem.current.RaycastAll(eventData, _raycastResults);
    }

    public void toggleScreen(string screenId)
    {
        if (_outsideClosedScreenId == screenId &&
            Time.frameCount <= _outsideClosedFrame + 1)
        {
            _outsideClosedScreenId = string.Empty;
            return;
        }

        for (int i = openScreens.Count - 1; i >= 0; i--)
        {
            UIScreen screen = openScreens[i];
            if (screen == null)
            {
                openScreens.RemoveAt(i);
                continue;
            }

            if (screen.screenId == screenId)
            {
                hideScreen(screen);
                return;
            }
        }
        showScreen(screenId);
    }

    public void showScreen(string screenId)
    {
        foreach (var screen in allScreens)
        {
            if (screen == null) continue;
            if (screen.screenId != screenId) continue;
            if (openScreens.Contains(screen)) return;
            ContextMenuController.instance.Close();
            screen.show();
            openScreens.Add(screen);
            return;
        }
    }

    public void hideScreen(string screenId)
    {
        foreach (var screen in allScreens)
        {
            if (screen == null) continue;
            if (screenId != screen.screenId) continue;
            hideScreen(screen);
            return;
        }
    }
    
    public void hideScreen(UIScreen screen)
    {
        if (screen == null || !openScreens.Contains(screen)) return;
        screen.hide();
        openScreens.Remove(screen);
    }

    private void CloseTopmost()
    {
        if (openScreens.Count > 0)
        {
            hideScreen(openScreens[^1]);
            return;
        }
        
        ContextMenuController.instance.Close();
    }
}
