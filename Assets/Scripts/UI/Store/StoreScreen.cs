using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class StoreScreen : UIScreen
{
    private const bool verbose = false;

    private enum StoreSection
    {
        Animals = 0,
        Buildings = 1
    }

    [SerializeField] private RectTransform storeItemsContainer;
    [SerializeField] private GameObject storeItemPrefab;

    [Header("Tabs")]
    [SerializeField] private Button animalsButton;
    [SerializeField] private Button buildingsButton;
    [SerializeField] private StoreSection defaultSection = StoreSection.Animals;

    [Header("Chicken")]
    [SerializeField] private string chickenItemId = "chicken";
    [SerializeField] private string chickenDisplayName = "Chicken";
    [SerializeField] private string chickenCoopTileId = "coop";
    [SerializeField] private Sprite chickenSprite;
    [SerializeField, Min(0)] private int chickenCost = 5;

    private readonly List<StoreItem> _createdStoreItems = new List<StoreItem>();
    private readonly List<StoreItemData> _visibleItems = new List<StoreItemData>();
    private StoreSection _selectedSection;
    private bool _animalsTabHooked;
    private bool _buildingsTabHooked;

    private void Awake()
    {
        _selectedSection = defaultSection;
        EnsureTabButtons();
        HookTabButtons();
    }

    public override void show()
    {
        base.show();
        EnsureTabButtons();
        HookTabButtons();
        RefreshStoreItems();
    }

    public bool ContainsScreenPoint(Vector2 screenPosition)
    {
        RectTransform rect = transform as RectTransform;
        Canvas canvas = GetComponentInParent<Canvas>();
        Camera eventCamera = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        return rect != null && RectTransformUtility.RectangleContainsScreenPoint(
            rect,
            screenPosition,
            eventCamera);
    }

    public void CloseForBuildingPlacement()
    {
        UISystem ui = UISystem.instance;
        if (ui != null)
        {
            ui.hideScreen(this);
        }

        if (gameObject.activeSelf)
        {
            hide();
        }
    }

    public void showAnimals()
    {
        SetSection(StoreSection.Animals);
    }

    public void showBuildings()
    {
        SetSection(StoreSection.Buildings);
    }

    public void ShowAnimals()
    {
        showAnimals();
    }

    public void ShowBuildings()
    {
        showBuildings();
    }

    private void SetSection(StoreSection section)
    {
        _selectedSection = section;
        RefreshStoreItems();
    }

    private void RefreshStoreItems()
    {
        _visibleItems.Clear();
        if (_selectedSection == StoreSection.Animals)
        {
            PopulateAnimalItems();
        }
        else
        {
            PopulateBuildingItems();
        }

        for (int i = 0; i < _createdStoreItems.Count; i++)
        {
            if (_createdStoreItems[i] != null)
            {
                _createdStoreItems[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < _visibleItems.Count; i++)
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

            _createdStoreItems[i].Setup(_visibleItems[i], this);
        }

        if (animalsButton != null)
        {
            animalsButton.interactable = _selectedSection != StoreSection.Animals;
        }

        if (buildingsButton != null)
        {
            buildingsButton.interactable = _selectedSection != StoreSection.Buildings;
        }
    }

    private void PopulateAnimalItems()
    {
        _visibleItems.Add(new StoreItemData(
            chickenItemId,
            chickenDisplayName,
            ResolveChickenSprite(),
            chickenCost));
    }

    private void PopulateBuildingItems()
    {
        RegistryService registries = RegistryService.instance;
        TileRegistrySO tileRegistry = registries != null ? registries.TileRegistry : null;
        if (tileRegistry == null)
        {
            if (verbose) Log.warning("[StoreScreen] Tile registry is unavailable.");
            return;
        }

        IReadOnlyList<TileDefinitionSO> definitions = tileRegistry.Tiles;
        for (int i = 0; i < definitions.Count; i++)
        {
            TileDefinitionSO definition = definitions[i];
            if (!IsStoreBuilding(definition))
            {
                continue;
            }

            StoreItemData item = StoreItemData.CreateBuilding(definition);
            if (item != null)
            {
                _visibleItems.Add(item);
            }
        }
    }

    private static bool IsStoreBuilding(TileDefinitionSO definition)
    {
        return definition != null &&
               definition.Factory != null &&
               definition.Sprite != null &&
               definition.Category != TileCategory.Terrain;
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

        StoreItem storeItem = createdObject.GetComponent<StoreItem>();
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

    private void EnsureTabButtons()
    {
        if (animalsButton != null && buildingsButton != null)
        {
            return;
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
            {
                continue;
            }

            if (animalsButton == null &&
                button.name.IndexOf("animal", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                animalsButton = button;
            }
            else if (buildingsButton == null &&
                     button.name.IndexOf("building", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                buildingsButton = button;
            }
        }
    }

    private void HookTabButtons()
    {
        if (!_animalsTabHooked && animalsButton != null)
        {
            animalsButton.onClick.AddListener(showAnimals);
            _animalsTabHooked = true;
        }

        if (!_buildingsTabHooked && buildingsButton != null)
        {
            buildingsButton.onClick.AddListener(showBuildings);
            _buildingsTabHooked = true;
        }
    }

    private void OnDestroy()
    {
        if (_animalsTabHooked && animalsButton != null)
        {
            animalsButton.onClick.RemoveListener(showAnimals);
        }

        if (_buildingsTabHooked && buildingsButton != null)
        {
            buildingsButton.onClick.RemoveListener(showBuildings);
        }
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
