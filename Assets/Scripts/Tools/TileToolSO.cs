using UnityEngine;

public abstract class TileToolSO : ScriptableObject
{
    [SerializeField] private string toolId;

    public string ToolId => toolId;

    public virtual void OnActivated(GameObject origin)
    {
    }

    public virtual void OnDeactivated(GameObject origin)
    {
    }

    public abstract bool CanApply(ToolUseContext context);
    public abstract bool Apply(ToolUseContext context);
}
