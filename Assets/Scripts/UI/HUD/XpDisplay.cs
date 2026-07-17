using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XpDisplay : MonoBehaviourSingleton<XpDisplay>
{
    [SerializeField] public TMP_Text levelText;
    [SerializeField] public GenericProgressBar xpProgressBar;
    [SerializeField] private RectTransform xpImage;
    [SerializeField, Min(0.05f)] private float bounceSeconds = 0.3f;

    private void Start()
    {
        UserModel user = UserModel.GetOrCreate();
        user.ExperienceChanged += HandleExperienceChanged;
        Refresh(user.Level, user.Experience, user.ExperienceToNextLevel);
        originalScale = xpImage.localScale;
    }

    public void GainExperience(int amount, Vector3 worldPosition)
    {
        if (amount <= 0) return;
        UserModel user = UserModel.GetOrCreate();
        Image image = xpImage != null ? xpImage.GetComponent<Image>() : null;
        UiRewardFlyer.Fly(image != null ? image.sprite : null, worldPosition,
            xpProgressBar.transform as RectTransform, () => user.AddExperience(amount));
    }

    private void HandleExperienceChanged(int newLevel, int currentXp, int requiredXp)
    {
        Refresh(newLevel, currentXp, requiredXp);
        if (xpImage != null) StartCoroutine(Bounce(xpImage));
    }

    private void Refresh(int newLevel, int currentXp, int requiredXp)
    {
        levelText.text = newLevel.ToString();
        xpProgressBar.SetValue(currentXp, requiredXp);
    }

    private IEnumerator Bounce(RectTransform target)
    {
        float elapsed = 0f;
        while (elapsed < bounceSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / bounceSeconds);
            target.localScale = originalScale * (1f + Mathf.Sin(progress * Mathf.PI) * 0.25f);
            yield return null;
        }
        target.localScale = originalScale;
    }

    private void OnDestroy()
    {
        if (UserModel.hasInstance) UserModel.instance.ExperienceChanged -= HandleExperienceChanged;
    }

    private Vector3 originalScale;
}
