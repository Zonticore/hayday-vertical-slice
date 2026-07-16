using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ToolController : MonoBehaviourSingleton<ToolController>
{
    private const bool verbose = false;

    private readonly HashSet<int> _targetsAppliedThisStroke = new HashSet<int>();
    private TileToolSO _activeTool;
    private GameObject _origin;

    public event Action<TileToolSO> ToolActivated;
    public event Action ToolDeactivated;

    public TileToolSO ActiveTool => _activeTool;
    public bool HasActiveTool => _activeTool != null;

    public void Activate(TileToolSO tool, GameObject origin)
    {
        if (tool == null) return;

        Deactivate();
        _activeTool = tool;
        _origin = origin;
        _targetsAppliedThisStroke.Clear();
        _activeTool.OnActivated(_origin);

        if (verbose) Log.info($"[ToolController] Activated '{tool.ToolId}'.");
        ToolActivated?.Invoke(tool);
    }

    public void BeginStroke()
    {
        _targetsAppliedThisStroke.Clear();
    }

    public bool TryApply(GameObject target)
    {
        if (_activeTool == null || target == null)
        {
            return false;
        }

        int targetId = target.GetInstanceID();
        if (_targetsAppliedThisStroke.Contains(targetId))
        {
            return false;
        }

        var context = new ToolUseContext(_origin, target);
        if (!_activeTool.CanApply(context))
        {
            return false;
        }

        bool applied = _activeTool.Apply(context);
        if (applied)
        {
            _targetsAppliedThisStroke.Add(targetId);
        }

        return applied;
    }

    public void EndStroke()
    {
        _targetsAppliedThisStroke.Clear();
    }

    public void Deactivate()
    {
        if (_activeTool == null) return;

        TileToolSO previous = _activeTool;
        GameObject previousOrigin = _origin;
        _activeTool = null;
        _origin = null;
        _targetsAppliedThisStroke.Clear();

        previous.OnDeactivated(previousOrigin);
        ToolDeactivated?.Invoke();
    }
}
