using UnityEngine;

public sealed class InteractionController : MonoBehaviourSingleton<InteractionController>
{
    private const bool verbose = false;

    public void Select(GameObject target, Vector2 screenPosition)
    {
        if (target == null)
        {
            ContextMenuController.instance.Close();
            return;
        }

        ContextActionSource source = target.GetComponentInParent<ContextActionSource>();
        if (source == null)
        {
            if (verbose) Log.info($"[InteractionController] '{target.name}' has no context actions.");
            ContextMenuController.instance.Close();
            return;
        }

        ContextMenuController.instance.Open(source, screenPosition);
    }
}
