using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ItemDisplay : MonoBehaviour
{
    private const bool verbose = false;

    [SerializeField] private Image image;
    [SerializeField] private TMP_Text quantityText;

    public string ItemId { get; private set; }
    public int Amount { get; private set; }
    public Sprite Sprite => image != null ? image.sprite : null;

    private void Awake()
    {
        EnsureReferences();
    }

    public void SetItem(string itemId, Sprite sprite, int amount = 1)
    {
        EnsureReferences();

        ItemId = itemId ?? string.Empty;
        Amount = Mathf.Max(0, amount);
        gameObject.name = string.IsNullOrWhiteSpace(ItemId)
            ? "ItemDisplay"
            : $"ItemDisplay_{ItemId}";

        if (image != null)
        {
            image.sprite = sprite;
            image.enabled = sprite != null;
            image.preserveAspect = true;
        }

        if (quantityText != null)
        {
            quantityText.text = Amount > 1 ? $"x{Amount}" : string.Empty;
            quantityText.gameObject.SetActive(Amount > 1);
        }

        gameObject.SetActive(true);
    }

    public void Clear()
    {
        ItemId = string.Empty;
        Amount = 0;

        if (image != null)
        {
            image.sprite = null;
            image.enabled = false;
        }

        if (quantityText != null)
        {
            quantityText.text = string.Empty;
            quantityText.gameObject.SetActive(false);
        }
    }

    private void EnsureReferences()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (quantityText == null)
        {
            quantityText = GetComponentInChildren<TMP_Text>(true);
        }

        if (quantityText == null)
        {
            CreateQuantityText();
        }

        if (image == null && verbose)
        {
            Log.warning($"[ItemDisplay] '{name}' requires an Image component.");
        }
    }

    private void CreateQuantityText()
    {
        var textObject = new GameObject(
            "Quantity",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        rect.anchorMin = new Vector2(0.45f, 0f);
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = new Vector2(-6f, -4f);

        quantityText = textObject.GetComponent<TextMeshProUGUI>();
        quantityText.alignment = TextAlignmentOptions.BottomRight;
        quantityText.fontSize = 28f;
        quantityText.fontStyle = FontStyles.Bold;
        quantityText.color = Color.white;
        quantityText.raycastTarget = false;
        quantityText.enableAutoSizing = true;
        quantityText.fontSizeMin = 12f;
        quantityText.fontSizeMax = 32f;
        quantityText.gameObject.SetActive(false);
    }
}
