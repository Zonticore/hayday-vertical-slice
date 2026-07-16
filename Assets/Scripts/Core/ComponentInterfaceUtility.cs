using UnityEngine;

public static class ComponentInterfaceUtility
{
    public static bool TryGetInterface<T>(GameObject target, out T result) where T : class
    {
        if (target != null)
        {
            MonoBehaviour[] components = target.GetComponents<MonoBehaviour>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] is T match)
                {
                    result = match;
                    return true;
                }
            }
        }

        result = null;
        return false;
    }
}
