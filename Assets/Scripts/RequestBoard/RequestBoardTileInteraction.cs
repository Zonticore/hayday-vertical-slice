using UnityEngine;

public sealed class RequestBoardTileInteraction : MonoBehaviour, IPrimaryInteraction
{
    private const bool verbose = false;

    private string _screenId = RequestBoardScreen.ScreenId;

    public void Initialize(string screenId)
    {
        _screenId = string.IsNullOrWhiteSpace(screenId)
            ? RequestBoardScreen.ScreenId
            : screenId.Trim();
    }

    public void Interact(Vector2 screenPosition)
    {
        UISystem ui = UISystem.instance;
        if (ui == null)
        {
            if (verbose) Log.warning("[RequestBoardTileInteraction] UISystem is unavailable.");
            return;
        }

        ui.showScreen(_screenId);
    }
}
