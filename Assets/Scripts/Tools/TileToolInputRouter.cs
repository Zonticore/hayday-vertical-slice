using UnityEngine;

public sealed class TileToolInputRouter : MonoBehaviour
{
    private const bool verbose = false;

    [SerializeField] private Camera worldCamera;
    [SerializeField] private LayerMask toolTargetLayers = ~0;

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }
    }

    public void BeginStroke(Vector2 screenPosition, bool applyAtStart = true)
    {
        ToolController controller = ToolController.instance;
        if (controller == null)
        {
            if (verbose) Log.warning("[TileToolInputRouter] No ToolController is available.");
            return;
        }

        controller.BeginStroke();
        if (applyAtStart)
        {
            ApplyAt(screenPosition);
        }
    }

    public void ContinueStroke(Vector2 screenPosition)
    {
        ApplyAt(screenPosition);
    }

    public void EndStroke()
    {
        ToolController.instance.EndStroke();
    }

    private void ApplyAt(Vector2 screenPosition)
    {
        ToolController controller = ToolController.instance;
        if (controller == null || !controller.HasActiveTool || worldCamera == null)
        {
            return;
        }

        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        Collider2D target = Physics2D.OverlapPoint(worldPosition, toolTargetLayers);

        if (target != null)
        {
            controller.TryApply(target.gameObject);
        }
    }
}
