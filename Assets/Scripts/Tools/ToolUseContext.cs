using UnityEngine;

public readonly struct ToolUseContext
{
    public GameObject Origin { get; }
    public GameObject Target { get; }

    public ToolUseContext(GameObject origin, GameObject target)
    {
        Origin = origin;
        Target = target;
    }
}
