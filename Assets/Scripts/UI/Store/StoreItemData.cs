using UnityEngine;

public sealed class StoreItemData
{
    public string ItemId { get; }
    public string DisplayName { get; }
    public Sprite Sprite { get; }
    public int Cost { get; }

    public StoreItemData(string itemId, string displayName, Sprite sprite, int cost)
    {
        ItemId = itemId ?? string.Empty;
        DisplayName = displayName ?? string.Empty;
        Sprite = sprite;
        Cost = Mathf.Max(0, cost);
    }
}
