using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinsDisplay : MonoBehaviourSingleton<CoinsDisplay>
{
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private RectTransform coinImage;
    [SerializeField, Min(0.05f)] private float bounceSeconds = 0.3f;

    protected override void Awake()
    {
        base.Awake();
        if (coinImage == null) coinImage = transform.Find("CoinsImage") as RectTransform;
    }

    private void Start()
    {
        UserModel user = UserModel.GetOrCreate();
        user.CoinsChanged += HandleCoinsChanged;
        UpdateCoinsCount(user.Coins);
    }

    public void GainCoins(int amount, Vector3 worldPosition)
    {
        if (amount <= 0) return;
        UserModel user = UserModel.GetOrCreate();
        Image image = coinImage != null ? coinImage.GetComponent<Image>() : null;
        UiRewardFlyer.Fly(image != null ? image.sprite : null, worldPosition, coinImage,
            () => user.AddCoins(amount));
    }

    public void UpdateCoinsCount(int newCount) => coinsText.text = $"{newCount:N0}";

    public void updateCoinsCount(int newCount) => UpdateCoinsCount(newCount);

    private void HandleCoinsChanged(int newCount)
    {
        UpdateCoinsCount(newCount);
        if (coinImage != null) StartCoroutine(Bounce(coinImage));
    }

    private IEnumerator Bounce(RectTransform target)
    {
        Vector3 original = target.localScale;
        float elapsed = 0f;
        while (elapsed < bounceSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / bounceSeconds);
            target.localScale = original * (1f + Mathf.Sin(progress * Mathf.PI) * 0.25f);
            yield return null;
        }
        target.localScale = original;
    }

    private void OnDestroy()
    {
        if (UserModel.hasInstance) UserModel.instance.CoinsChanged -= HandleCoinsChanged;
    }
}
