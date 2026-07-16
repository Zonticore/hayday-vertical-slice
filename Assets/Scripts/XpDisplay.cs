using TMPro;
using UnityEngine;

public class XpDisplay : MonoBehaviourSingleton<XpDisplay>
{
    [SerializeField] public TMP_Text levelText;
    [SerializeField] public GenericProgressBar xpProgressBar;
}