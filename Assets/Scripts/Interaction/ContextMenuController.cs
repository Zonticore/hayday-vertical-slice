using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ContextMenuController : MonoBehaviourSingleton<ContextMenuController>
{
    private const bool verbose = false;

    private IReadOnlyList<ContextAction> _currentActions = Array.Empty<ContextAction>();

    public event Action<IReadOnlyList<ContextAction>, Vector2> Opened;
    public event Action Closed;

    public IReadOnlyList<ContextAction> CurrentActions => _currentActions;
    public bool IsOpen { get; private set; }

    public void Open(ContextActionSource source, Vector2 screenPosition)
    {
        if (source == null)
        {
            Close();
            return;
        }

        _currentActions = source.GetAvailableActions();
        if (_currentActions.Count == 0)
        {
            Close();
            return;
        }

        IsOpen = true;
        Opened?.Invoke(_currentActions, screenPosition);

        if (verbose)
        {
            Log.info($"[ContextMenuController] Opened with {_currentActions.Count} actions at {screenPosition}.");
        }
    }

    public void Execute(int actionIndex)
    {
        if (!IsOpen || actionIndex < 0 || actionIndex >= _currentActions.Count)
        {
            return;
        }

        ContextAction action = _currentActions[actionIndex];
        bool executed = action.TryExecute();

        if (verbose)
        {
            Log.info($"[ContextMenuController] Action '{action.ActionId}' executed: {executed}.");
        }

        if (executed && action.CloseMenuOnExecute)
        {
            Close();
        }
    }

    public void Close()
    {
        if (!IsOpen && _currentActions.Count == 0) return;

        IsOpen = false;
        _currentActions = Array.Empty<ContextAction>();
        Closed?.Invoke();

        if (verbose) Log.info("[ContextMenuController] Closed.");
    }
}
