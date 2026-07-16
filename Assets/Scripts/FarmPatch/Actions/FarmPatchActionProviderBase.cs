using UnityEngine;

public abstract class FarmPatchActionProviderBase : MonoBehaviour, IContextActionProvider
{
    protected FarmPatchState Patch { get; private set; }
    protected ContextActionDefinitionSO Definition { get; private set; }

    public void Initialize(
        FarmPatchState patch,
        ContextActionDefinitionSO definition)
    {
        Patch = patch;
        Definition = definition;
    }

    public abstract void CollectActions(ContextActionCollection collection);

    protected void AddToolAction(ContextActionCollection collection)
    {
        if (Definition == null) return;

        bool isEnabled = Definition.Tool != null && ToolController.instance != null;
        collection.Add(Definition.CreateRuntimeAction(isEnabled, ActivateTool));
    }

    private void ActivateTool()
    {
        if (Definition == null || Definition.Tool == null)
        {
            return;
        }

        ToolController.instance.Activate(Definition.Tool, gameObject);
    }
}
