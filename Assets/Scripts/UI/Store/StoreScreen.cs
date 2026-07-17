using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class StoreScreen : UIScreen
{
    private const bool verbose = false;

    [SerializeField] private RectTransform storeItemsContainer;
    [SerializeField] private GameObject storeItemPrefab;

    [Header("Chicken")]
    [SerializeField] private string chickenItemId = "chicken";
    [SerializeField] private string chickenDisplayName = "Chicken";
    [SerializeField] private string chickenCoopTileId = "coop";
    [SerializeField] private Sprite chickenSprite;
    [SerializeField, Min(0)] private int chickenCost = 5;

    private readonly List<StoreItem> _createdStoreItems = new List<StoreItem>();

    public override void show()
    {
        base.show();
        UpdateUi();
    }

    private void UpdateUi()
    {
        for (int i = 0; i < _createdStoreItems.Count; i++)
        {
            if (_createdStoreItems[i] != null)
            {
                _createdStoreItems[i].gameObject.SetActive(false);
            }
        }

        var itemsToPopulate = new List<StoreItemData>
        {
            new StoreItemData(
                chickenItemId,
                chickenDisplayName,
                ResolveChickenSprite(),
                chickenCost)
        };

        for (int i = 0; i < itemsToPopulate.Count; i++)
        {
            if (_createdStoreItems.Count <= i)
            {
                StoreItem created = CreateStoreItem();
                if (created == null)
                {
                    continue;
                }

                _createdStoreItems.Add(created);
            }

            _createdStoreItems[i].Setup(itemsToPopulate[i]);
        }
    }

    private StoreItem CreateStoreItem()
    {
        if (storeItemsContainer == null)
        {
            if (verbose) Log.warning("[StoreScreen] Store Items Container is not assigned.");
            return null;
        }

        GameObject createdObject;
        if (storeItemPrefab != null)
        {
            createdObject = Instantiate(storeItemPrefab, storeItemsContainer, false);
        }
        else
        {
            createdObject = new GameObject(
                "StoreItem",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(StoreItem));
            createdObject.transform.SetParent(storeItemsContainer, false);
            ((RectTransform)createdObject.transform).sizeDelta = new Vector2(180f, 220f);
        }

        var storeItem = createdObject.GetComponent<StoreItem>();
        return storeItem != null ? storeItem : createdObject.AddComponent<StoreItem>();
    }

    private Sprite ResolveChickenSprite()
    {
        if (chickenSprite != null)
        {
            return chickenSprite;
        }

        RegistryService registries = RegistryService.instance;
        if (registries != null &&
            registries.TileRegistry != null &&
            registries.TileRegistry.TryGet(chickenCoopTileId, out TileDefinitionSO coop) &&
            coop.Factory is ChickenCoopTileFactorySO coopFactory)
        {
            chickenSprite = coopFactory.ChickenSprite;
        }

        return chickenSprite;
    }

    private void OnValidate()
    {
        chickenItemId = chickenItemId == null ? string.Empty : chickenItemId.Trim();
        chickenDisplayName = chickenDisplayName == null
            ? string.Empty
            : chickenDisplayName.Trim();
        chickenCoopTileId = chickenCoopTileId == null
            ? string.Empty
            : chickenCoopTileId.Trim();
        chickenCost = Mathf.Max(0, chickenCost);
    }
}
