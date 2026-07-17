using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Farm/Request Board/Board Config",
    fileName = "RequestBoardConfig_")]
public sealed class RequestBoardConfigSO : ScriptableObject
{
    [SerializeField] private List<RequestOrderSO> orders =
        new List<RequestOrderSO>();
    [SerializeField] private Sprite goldSprite;
    [SerializeField] private Sprite experienceSprite;
    [SerializeField, Min(0f)] private float orderCooldownSeconds = 5f;
    [SerializeField] private bool avoidImmediateRepeat;

    public IReadOnlyList<RequestOrderSO> Orders => orders;
    public Sprite GoldSprite => goldSprite;
    public Sprite ExperienceSprite => experienceSprite;
    public float OrderCooldownSeconds => Mathf.Max(0f, orderCooldownSeconds);
    public bool AvoidImmediateRepeat => avoidImmediateRepeat;

    private void OnValidate()
    {
        orderCooldownSeconds = Mathf.Max(0f, orderCooldownSeconds);
    }
}
