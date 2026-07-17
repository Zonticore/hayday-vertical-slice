
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GenericProgressBar : MonoBehaviour
{
    [SerializeField] public TMP_Text progressText;
    [SerializeField] public Slider progressSlider;

    public void SetValue(int current, int maximum)
    {
        int safeMaximum = Mathf.Max(1, maximum);
        int safeCurrent = Mathf.Clamp(current, 0, safeMaximum);
        progressSlider.minValue = 0f;
        progressSlider.maxValue = safeMaximum;
        progressSlider.value = safeCurrent;
        progressText.text = $"{safeCurrent:N0} / {safeMaximum:N0}";
    }
}
