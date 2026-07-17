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

    public static bool TryGetInterfaceInParent<T>(GameObject target, out T result) where T : class
    {
        Transform current = target != null ? target.transform : null;
        while (current != null)
        {
            if (TryGetInterface(current.gameObject, out result))
            {
                return true;
            }

            current = current.parent;
        }

        result = null;
        return false;
    }
}
