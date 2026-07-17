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

    [Header("Current Order")]
    [SerializeField] private string requestedItemId = "wheat";
    [SerializeField] private StorageType requestedStorage = StorageType.Grain;
    [SerializeField] private Sprite requestedItemSprite;
    [SerializeField, Min(1)] private int requestedAmount = 3;

    [Header("Rewards")]
    [SerializeField] private Sprite goldSprite;
    [SerializeField, Min(0)] private int goldReward = 10;
    [SerializeField] private Sprite experienceSprite;
    [SerializeField, Min(0)] private int experienceReward = 5;
    [SerializeField, Min(0f)] private float orderCooldownSeconds = 5f;

    private readonly List<ItemDisplay> _createdRequestedItems = new List<ItemDisplay>();
    private readonly List<ItemDisplay> _createdRewardItems = new List<ItemDisplay>();

    private UserModel _user;
    private float _nextOrderAt;
    private bool _subscribed;

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
    }

    private void OnEnable()
    {
        Subscribe();
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
            UpdateUi();
            return;
        }

        UpdateTimerText();
    }

    public override void show()
    {
        base.show();
        Subscribe();
        UpdateUi();
    }

    public void doOrder()
    {
        if (_nextOrderAt > Time.unscaledTime)
        {
            return;
        }

        _user = UserModel.GetOrCreate();
        if (!_user.TryRemoveItem(requestedStorage, requestedItemId, requestedAmount))
        {
            UpdateUi();
            return;
        }

        _user.AddCoins(goldReward);
        _user.AddExperience(experienceReward);
        _nextOrderAt = orderCooldownSeconds > 0f
            ? Time.unscaledTime + orderCooldownSeconds
            : 0f;

        UpdateUi();

        if (verbose)
        {
            Log.info($"[RequestBoardScreen] Completed an order for {requestedAmount} {requestedItemId}.");
        }
    }

    private void UpdateUi()
    {
        _user = UserModel.GetOrCreate();

        var requested = new List<DisplayEntry>
        {
            new DisplayEntry(requestedItemId, requestedItemSprite, requestedAmount)
        };

        var rewards = new List<DisplayEntry>();
        if (goldReward > 0)
        {
            rewards.Add(new DisplayEntry("gold", goldSprite, goldReward));
        }

        if (experienceReward > 0)
        {
            rewards.Add(new DisplayEntry("xp", experienceSprite, experienceReward));
        }

        Populate(requestedItemsContainer, requested, _createdRequestedItems);
        Populate(rewardsContainer, rewards, _createdRewardItems);

        bool cooldownComplete = _nextOrderAt <= Time.unscaledTime;
        bool hasItems = _user.GetStorage(requestedStorage).GetQuantity(requestedItemId) >=
                        requestedAmount;
        if (submitButton != null)
        {
            submitButton.interactable = cooldownComplete && hasItems;
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
        if (storage == requestedStorage && itemId == requestedItemId)
        {
            UpdateUi();
        }
    }

    private void OnValidate()
    {
        requestedItemId = requestedItemId == null ? string.Empty : requestedItemId.Trim();
        requestedAmount = Mathf.Max(1, requestedAmount);
        goldReward = Mathf.Max(0, goldReward);
        experienceReward = Mathf.Max(0, experienceReward);
        orderCooldownSeconds = Mathf.Max(0f, orderCooldownSeconds);
    }
}
