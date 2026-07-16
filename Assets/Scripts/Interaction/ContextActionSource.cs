using System.Collections.Generic;
using UnityEngine;

public sealed class ContextActionSource : MonoBehaviour
{
    public IReadOnlyList<ContextAction> GetAvailableActions()
    {
        var collection = new ContextActionCollection();
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] is IContextActionProvider provider)
            {
                provider.CollectActions(collection);
            }
        }

        collection.Sort();
        return collection.Actions;
    }
}
