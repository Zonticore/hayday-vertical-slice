using System;
using UnityEngine;

public sealed class ContextAction
{
    private readonly Action _execute;

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
        _execute = execute;
    }

    public bool TryExecute()
    {
        if (!IsEnabled || _execute == null)
        {
            return false;
        }

        _execute.Invoke();
        return true;
    }
}
