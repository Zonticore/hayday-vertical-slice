using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Items/Item", fileName = "Item_")]
public sealed class ItemDefinitionSO : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private StorageType storage = StorageType.Barn;
    [SerializeField] private Sprite sprite;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public StorageType Storage => storage;
    public Sprite Sprite => sprite;

    private void OnValidate()
    {
        itemId = itemId == null ? string.Empty : itemId.Trim();
        displayName = displayName == null ? string.Empty : displayName.Trim();
    }
}
