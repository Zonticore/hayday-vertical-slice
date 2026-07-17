using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class UISystem : MonoBehaviourSingleton<UISystem>
{
    private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

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
        if (IsContextMenuOpen && pointer != null && pointer.press.wasPressedThisFrame &&
            !IsPointerOverContextOption(pointer.position.ReadValue()))
        {
            ContextMenuController.instance.Close();
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

    private void Raycast(Vector2 screenPosition)
    {
        _raycastResults.Clear();
        if (EventSystem.current == null) return;
        var eventData = new PointerEventData(EventSystem.current) { position = screenPosition };
        EventSystem.current.RaycastAll(eventData, _raycastResults);
    }

    public void toggleScreen(string screenId)
    {
        foreach (var screen in openScreens)
        {
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
            if (screenId != screen.screenId) continue;
            hideScreen(screen);
            return;
        }
    }
    
    public void hideScreen(UIScreen screen)
    {
        if (!openScreens.Contains(screen)) return;
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
