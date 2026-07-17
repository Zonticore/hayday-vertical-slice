using UnityEngine;

[CreateAssetMenu(
    menuName = "Farm/Production/Recipe",
    fileName = "ProductionRecipe_")]
public sealed class ProductionRecipeSO : ScriptableObject
{
    [SerializeField] private string recipeId;
    [SerializeField] private string displayName;

    [Header("Input")]
    [SerializeField] private ItemDefinitionSO inputItem;
    [SerializeField, Min(1)] private int inputAmount = 1;

    [Header("Output")]
    [SerializeField] private ItemDefinitionSO outputItem;
    [SerializeField, Min(1)] private int outputAmount = 1;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float productionSeconds = 15f;

    public string RecipeId => recipeId;
    public string DisplayName => displayName;
    public string InputItemId => inputItem != null ? inputItem.ItemId : string.Empty;
    public StorageType InputStorage => inputItem != null
        ? inputItem.Storage
        : StorageType.Barn;
    public int InputAmount => inputAmount;
    public string OutputItemId => outputItem != null ? outputItem.ItemId : string.Empty;
    public StorageType OutputStorage => outputItem != null
        ? outputItem.Storage
        : StorageType.Barn;
    public int OutputAmount => outputAmount;
    public Sprite OutputSprite => outputItem != null ? outputItem.Sprite : null;
    public float ProductionSeconds => productionSeconds;

    private void OnValidate()
    {
        recipeId = recipeId == null ? string.Empty : recipeId.Trim();
        displayName = displayName == null ? string.Empty : displayName.Trim();
        inputAmount = Mathf.Max(1, inputAmount);
        outputAmount = Mathf.Max(1, outputAmount);
        productionSeconds = Mathf.Max(0f, productionSeconds);
    }
}
