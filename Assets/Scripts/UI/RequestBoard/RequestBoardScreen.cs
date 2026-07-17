using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RequestBoardScreen : UIScreen
{
    public const string ScreenId = "request_board";

    private const bool verbose = false;

    [Header("Containers")]
    [SerializeField] private RectTransform requestedItemsContainer;
    [SerializeField] private RectTransform rewardsContainer;
    [SerializeField] private GameObject itemDisplayPrefab;
    [SerializeField] private TMP_Text timeUntilNextOrder;
    [SerializeField] private Button submitButton;

    [Header("Content")]
    [SerializeField] private RequestBoardConfigSO config;

    private readonly List<ItemDisplay> _createdRequestedItems =
        new List<ItemDisplay>();
    private readonly List<ItemDisplay> _createdRewardItems =
        new List<ItemDisplay>();
    private readonly List<RequestOrderSO> _eligibleOrders =
        new List<RequestOrderSO>();
    private readonly Dictionary<ItemDefinitionSO, int> _requiredAmounts =
        new Dictionary<ItemDefinitionSO, int>();

    private UserModel _user;
    private RequestOrderSO _currentOrder;
    private RequestOrderSO _previousOrder;
    private int _currentGoldReward;
    private int _currentExperienceReward;
    private float _nextOrderAt;
    private bool _subscribed;
    private bool _hasRewardWorldPosition;
    private Vector3 _rewardWorldPosition;

    private readonly struct DisplayEntry
    {
        public string ItemId { get; }
        public Sprite Sprite { get; }
        public int Amount { get; }

        public DisplayEntry(string itemId, Sprite sprite, int amount)
        {
            ItemId = itemId;
            Sprite = sprite;
            Amount = amount;
        }
    }

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(screenId))
        {
            screenId = ScreenId;
        }

        EnsureCurrentOrder();
    }

    private void OnEnable()
    {
        Subscribe();
        EnsureCurrentOrder();
        UpdateUi();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        if (_nextOrderAt <= 0f)
        {
            return;
        }

        if (Time.unscaledTime >= _nextOrderAt)
        {
            _nextOrderAt = 0f;
            EnsureCurrentOrder();
            UpdateUi();
            return;
        }

        UpdateTimerText();
    }

    public override void show()
    {
        base.show();
        Subscribe();
        EnsureCurrentOrder();
        UpdateUi();
    }

    public void SetRewardWorldPosition(Vector3 worldPosition)
    {
        _rewardWorldPosition = worldPosition;
        _hasRewardWorldPosition = true;
    }

    public void doOrder()
    {
        if (_nextOrderAt > Time.unscaledTime || _currentOrder == null)
        {
            return;
        }

        _user = UserModel.GetOrCreate();
        if (!HasRequiredItems(_currentOrder))
        {
            UpdateUi();
            return;
        }

        BuildRequiredAmounts(_currentOrder);
        var requirements =
            new List<KeyValuePair<ItemDefinitionSO, int>>(_requiredAmounts);
        foreach (KeyValuePair<ItemDefinitionSO, int> requirement in
                 requirements)
        {
            ItemDefinitionSO item = requirement.Key;
            _user.TryRemoveItem(
                item.Storage,
                item.ItemId,
                requirement.Value);
        }

        GrantRewards(_currentGoldReward, _currentExperienceReward);

        if (verbose)
        {
            Log.info($"[RequestBoardScreen] Completed order '{_currentOrder.OrderId}'.");
        }

        _previousOrder = _currentOrder;
        _currentOrder = null;
        _currentGoldReward = 0;
        _currentExperienceReward = 0;
        float cooldown = config != null ? config.OrderCooldownSeconds : 0f;
        _nextOrderAt = cooldown > 0f
            ? Time.unscaledTime + cooldown
            : 0f;

        EnsureCurrentOrder();
        UpdateUi();
    }

    private void GrantRewards(int goldReward, int experienceReward)
    {
        CoinsDisplay coinsDisplay = CoinsDisplay.instance;
        if (goldReward > 0)
        {
            if (coinsDisplay != null && _hasRewardWorldPosition)
            {
                coinsDisplay.GainCoins(goldReward, _rewardWorldPosition);
            }
            else
            {
                _user.AddCoins(goldReward);
            }
        }

        XpDisplay xpDisplay = XpDisplay.instance;
        if (experienceReward > 0)
        {
            if (xpDisplay != null && _hasRewardWorldPosition)
            {
                xpDisplay.GainExperience(experienceReward, _rewardWorldPosition);
            }
            else
            {
                _user.AddExperience(experienceReward);
            }
        }
    }

    private void EnsureCurrentOrder()
    {
        if (_currentOrder != null || _nextOrderAt > Time.unscaledTime ||
            config == null)
        {
            return;
        }

        _eligibleOrders.Clear();
        IReadOnlyList<RequestOrderSO> orders = config.Orders;
        for (int i = 0; i < orders.Count; i++)
        {
            RequestOrderSO order = orders[i];
            if (order != null && order.RequestedItems.Count > 0)
            {
                _eligibleOrders.Add(order);
            }
        }

        if (_eligibleOrders.Count == 0)
        {
            if (verbose) Log.warning("[RequestBoardScreen] No valid orders are configured.");
            return;
        }

        if (config.AvoidImmediateRepeat && _eligibleOrders.Count > 1 &&
            _previousOrder != null)
        {
            _eligibleOrders.Remove(_previousOrder);
        }

        _currentOrder = _eligibleOrders[
            Random.Range(0, _eligibleOrders.Count)];
        _currentGoldReward = _currentOrder.RollGoldReward();
        _currentExperienceReward = _currentOrder.RollExperienceReward();
    }

    private bool HasRequiredItems(RequestOrderSO order)
    {
        if (_user == null || order == null)
        {
            return false;
        }

        BuildRequiredAmounts(order);
        foreach (KeyValuePair<ItemDefinitionSO, int> requirement in
                 _requiredAmounts)
        {
            ItemDefinitionSO item = requirement.Key;
            if (_user.GetStorage(item.Storage).GetQuantity(item.ItemId) <
                requirement.Value)
            {
                return false;
            }
        }

        return _requiredAmounts.Count > 0;
    }

    private void BuildRequiredAmounts(RequestOrderSO order)
    {
        _requiredAmounts.Clear();
        if (order == null)
        {
            return;
        }

        IReadOnlyList<RequestOrderItem> items = order.RequestedItems;
        for (int i = 0; i < items.Count; i++)
        {
            RequestOrderItem requested = items[i];
            ItemDefinitionSO item = requested?.Item;
            if (item == null)
            {
                continue;
            }

            _requiredAmounts.TryGetValue(item, out int currentAmount);
            _requiredAmounts[item] = currentAmount + requested.Amount;
        }
    }

    private void UpdateUi()
    {
        _user = UserModel.GetOrCreate();

        var requestedEntries = new List<DisplayEntry>();
        if (_currentOrder != null)
        {
            IReadOnlyList<RequestOrderItem> items = _currentOrder.RequestedItems;
            for (int i = 0; i < items.Count; i++)
            {
                RequestOrderItem requested = items[i];
                ItemDefinitionSO item = requested?.Item;
                if (item != null)
                {
                    requestedEntries.Add(new DisplayEntry(
                        item.ItemId,
                        item.Sprite,
                        requested.Amount));
                }
            }
        }

        var rewardEntries = new List<DisplayEntry>();
        if (_currentOrder != null && config != null)
        {
            if (_currentGoldReward > 0)
            {
                rewardEntries.Add(new DisplayEntry(
                    "gold",
                    config.GoldSprite,
                    _currentGoldReward));
            }

            if (_currentExperienceReward > 0)
            {
                rewardEntries.Add(new DisplayEntry(
                    "xp",
                    config.ExperienceSprite,
                    _currentExperienceReward));
            }
        }

        Populate(requestedItemsContainer, requestedEntries, _createdRequestedItems);
        Populate(rewardsContainer, rewardEntries, _createdRewardItems);

        if (submitButton != null)
        {
            submitButton.interactable =
                _nextOrderAt <= Time.unscaledTime &&
                HasRequiredItems(_currentOrder);
        }

        UpdateTimerText();
    }

    private void Populate(
        RectTransform container,
        IReadOnlyList<DisplayEntry> entries,
        List<ItemDisplay> createdDisplays)
    {
        if (container == null)
        {
            return;
        }

        for (int i = 0; i < createdDisplays.Count; i++)
        {
            if (createdDisplays[i] != null)
            {
                createdDisplays[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (createdDisplays.Count <= i)
            {
                ItemDisplay created = CreateItemDisplay(container);
                if (created == null)
                {
                    continue;
                }

                createdDisplays.Add(created);
            }

            DisplayEntry entry = entries[i];
            createdDisplays[i].SetItem(entry.ItemId, entry.Sprite, entry.Amount);
        }
    }

    private ItemDisplay CreateItemDisplay(RectTransform container)
    {
        GameObject createdObject;
        if (itemDisplayPrefab != null)
        {
            createdObject = Instantiate(itemDisplayPrefab, container, false);
        }
        else
        {
            createdObject = new GameObject(
                "ItemDisplay",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(ItemDisplay));
            createdObject.transform.SetParent(container, false);
        }

        RectTransform displayRect = (RectTransform)createdObject.transform;
        displayRect.anchorMin = new Vector2(0.5f, 0.5f);
        displayRect.anchorMax = new Vector2(0.5f, 0.5f);
        displayRect.sizeDelta = new Vector2(128f, 128f);

        var item = createdObject.GetComponent<ItemDisplay>();
        return item != null ? item : createdObject.AddComponent<ItemDisplay>();
    }

    private void UpdateTimerText()
    {
        if (timeUntilNextOrder == null)
        {
            return;
        }

        float remaining = Mathf.Max(0f, _nextOrderAt - Time.unscaledTime);
        timeUntilNextOrder.text = remaining > 0f
            ? $"Next order in {Mathf.CeilToInt(remaining)}s"
            : string.Empty;
    }

    private void Subscribe()
    {
        if (_subscribed)
        {
            return;
        }

        _user = UserModel.GetOrCreate();
        _user.ItemQuantityChanged += HandleItemQuantityChanged;
        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed || _user == null)
        {
            return;
        }

        _user.ItemQuantityChanged -= HandleItemQuantityChanged;
        _subscribed = false;
    }

    private void HandleItemQuantityChanged(
        StorageType storage,
        string itemId,
        int previous,
        int current)
    {
        if (_currentOrder == null)
        {
            return;
        }

        IReadOnlyList<RequestOrderItem> items = _currentOrder.RequestedItems;
        for (int i = 0; i < items.Count; i++)
        {
            ItemDefinitionSO requested = items[i]?.Item;
            if (requested != null && requested.Storage == storage &&
                requested.ItemId == itemId)
            {
                UpdateUi();
                return;
            }
        }
    }
}
