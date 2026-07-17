using System;
using UnityEngine;

public sealed class ContextAction
{
    private readonly Func<bool> _tryExecute;

    public string ActionId { get; }
    public string DisplayName { get; }
    public Sprite Icon { get; }
    public int Order { get; }
    public bool IsEnabled { get; }
    public bool CloseMenuOnExecute { get; }
    public TileToolSO Tool { get; }

    public ContextAction(
        string actionId,
        string displayName,
        Sprite icon,
        int order,
        bool isEnabled,
        bool closeMenuOnExecute,
        TileToolSO tool,
        Action execute)
    {
        ActionId = actionId;
        DisplayName = displayName;
        Icon = icon;
        Order = order;
        IsEnabled = isEnabled;
        CloseMenuOnExecute = closeMenuOnExecute;
        Tool = tool;
        _tryExecute = execute == null
            ? null
            : () =>
            {
                execute.Invoke();
                return true;
            };
    }

    public ContextAction(
        string actionId,
        string displayName,
        Sprite icon,
        int order,
        bool isEnabled,
        bool closeMenuOnExecute,
        TileToolSO tool,
        Func<bool> tryExecute)
    {
        ActionId = actionId;
        DisplayName = displayName;
        Icon = icon;
        Order = order;
        IsEnabled = isEnabled;
        CloseMenuOnExecute = closeMenuOnExecute;
        Tool = tool;
        _tryExecute = tryExecute;
    }

    public bool TryExecute()
    {
        if (!IsEnabled || _tryExecute == null)
        {
            return false;
        }

        return _tryExecute.Invoke();
    }
}
