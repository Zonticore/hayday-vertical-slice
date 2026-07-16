using UnityEngine;
using UnityEngine.EventSystems;

public sealed class PointerInteractable : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        InteractionController.instance.Select(gameObject, eventData.position);
    }
}
