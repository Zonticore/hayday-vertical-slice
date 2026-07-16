
using TMPro;
using UnityEngine;

public class CoinsDisplay : MonoBehaviourSingleton<CoinsDisplay>
{
    [SerializeField] private TMP_Text coinsText;
    
    public void updateCoinsCount(int newCount)
    {
        coinsText.text = $"{newCount:N0}";
    }
}