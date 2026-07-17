using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Interaction/Context Action", fileName = "ContextAction_")]
public sealed class ContextActionDefinitionSO : ScriptableObject
{
    [SerializeField] private string actionId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private int order;
    [SerializeField] private bool closeMenuOnExecute = true;
    [SerializeField] private TileToolSO tool;

    public string ActionId => actionId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public int Order => order;
    public bool CloseMenuOnExecute => closeMenuOnExecute;
    public TileToolSO Tool => tool;

    public ContextAction CreateRuntimeAction(bool isEnabled, Action execute)
    {
        return new ContextAction(
            actionId,
            displayName,
            icon,
            order,
            isEnabled,
            closeMenuOnExecute,
            tool,
            execute);
    }

    public ContextAction CreateRuntimeActionWithResult(
        bool isEnabled,
        Func<bool> tryExecute)
    {
        return new ContextAction(
            actionId,
            displayName,
            icon,
            order,
            isEnabled,
            closeMenuOnExecute,
            tool,
            tryExecute);
    }

    private void OnValidate()
    {
        actionId = actionId == null ? string.Empty : actionId.Trim();
        displayName = displayName == null ? string.Empty : displayName.Trim();
    }
}
