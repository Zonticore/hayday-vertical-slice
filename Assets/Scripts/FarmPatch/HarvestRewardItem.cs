using UnityEngine;

public sealed class HarvestRewardItem : MonoBehaviour
{
    private const bool verbose = false;

    private string _itemId;
    private int _amount;
    private StorageType _storageType;
    private float _lifetimeSeconds;
    private float _elapsed;
    private float _hopHeight;
    private Vector3 _startPosition;
    private Vector3 _startScale;
    private bool _rewardGranted;
    private Sprite _sprite;

    public void Initialize(
        string itemId,
        int amount,
        StorageType storageType,
        float lifetimeSeconds,
        float hopHeight)
    {
        _itemId = itemId;
        _amount = Mathf.Max(0, amount);
        _storageType = storageType;
        _lifetimeSeconds = Mathf.Max(0.05f, lifetimeSeconds);
        _hopHeight = Mathf.Max(0f, hopHeight);
        _startPosition = transform.position;
        _startScale = transform.localScale;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        _sprite = spriteRenderer != null ? spriteRenderer.sprite : null;
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsed / _lifetimeSeconds);
        float hop = Mathf.Sin(progress * Mathf.PI) * _hopHeight;
        transform.position = _startPosition + Vector3.up * hop;

        float scale = progress < 0.15f
            ? Mathf.SmoothStep(0.35f, 1f, progress / 0.15f)
            : Mathf.SmoothStep(1f, 0.35f, Mathf.InverseLerp(0.75f, 1f, progress));
        transform.localScale = _startScale * scale;

        if (_elapsed >= _lifetimeSeconds)
        {
            FlyToStorage();
            Destroy(gameObject);
        }
    }

    private void FlyToStorage()
    {
        StorageHudDisplay display = StorageHudDisplay.instance;
        if (display != null)
            display.FlyItem(_sprite, transform.position, _storageType, GrantReward);
        else
            GrantReward();
    }

    private void GrantReward()
    {
        if (_rewardGranted)
        {
            return;
        }

        _rewardGranted = true;
        UserModel user = UserModel.instance;
        int accepted = user != null
            ? user.AddItem(_storageType, _itemId, _amount)
            : 0;

        if (verbose)
        {
            Log.info($"[HarvestRewardItem] Collected {accepted}/{_amount} '{_itemId}'.");
        }
    }
}
