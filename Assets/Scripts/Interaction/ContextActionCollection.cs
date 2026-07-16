using System.Collections.Generic;

public sealed class ContextActionCollection
{
    private readonly List<ContextAction> _actions = new List<ContextAction>();

    public IReadOnlyList<ContextAction> Actions => _actions;

    public void Add(ContextAction action)
    {
        if (action != null)
        {
            _actions.Add(action);
        }
    }

    public void Sort()
    {
        _actions.Sort((left, right) => left.Order.CompareTo(right.Order));
    }
}
