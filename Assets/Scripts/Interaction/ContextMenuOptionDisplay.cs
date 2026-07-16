using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ContextMenuOptionDisplay : MonoBehaviour
{
    private const bool verbose = false;

    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text displayNameText;

    private Action _onSelected;

    public void Bind(ContextAction action, Action onSelected)
    {
        if (action == null)
        {
            if (verbose) Log.warning("[ContextMenuOptionDisplay] Cannot bind a null action.");
            gameObject.SetActive(false);
            return;
        }

        _onSelected = onSelected;
        gameObject.name = $"ContextOption_{action.ActionId}";

        if (displayNameText != null)
        {
            displayNameText.text = action.DisplayName;
        }

        if (iconImage != null)
        {
            iconImage.sprite = action.Icon;
            iconImage.enabled = action.Icon != null;
        }

        if (button != null)
        {
            button.interactable = action.IsEnabled;
            button.onClick.RemoveListener(HandleSelected);
            button.onClick.AddListener(HandleSelected);
        }
        else if (verbose)
        {
            Log.warning($"[ContextMenuOptionDisplay] '{name}' has no Button assigned.");
        }
    }

    private void HandleSelected()
    {
        _onSelected?.Invoke();
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleSelected);
        }

        _onSelected = null;
    }

    private void Reset()
    {
        button = GetComponent<Button>();
    }
}
