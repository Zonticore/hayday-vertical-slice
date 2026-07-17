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

        for (int i = 0; i < ui.allScreens.Count; i++)
        {
            if (ui.allScreens[i] is RequestBoardScreen requestBoardScreen &&
                requestBoardScreen.screenId == _screenId)
            {
                requestBoardScreen.SetRewardWorldPosition(transform.position);
                break;
            }
        }

        ui.showScreen(_screenId);
    }
}
