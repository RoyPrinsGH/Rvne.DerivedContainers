namespace Rvne.DerivedContainers;

/// <summary>
/// Represents a tree node that stores a value and optional child nodes.
/// </summary>
/// <typeparam name="T">The value type stored by each node.</typeparam>
public class Tree<T>
{
    /// <summary>
    /// Child nodes for the current node.
    /// </summary>
    public List<Tree<T>>? Children = null;

    /// <summary>
    /// Value stored in the current node.
    /// </summary>
    public T? Value = default;

    /// <summary>
    /// Projects each node value in the tree using <paramref name="map" /> and returns a tree with the same structure.
    /// </summary>
    /// <param name="map">Projection function applied to each <typeparamref name="T" /> value.</param>
    /// <returns>A tree containing projected values of type <typeparamref name="R" />.</returns>
    protected Tree<R> Map<R>(Func<T, R> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return MapCore(this);

        Tree<R> MapCore(Tree<T> node) => new()
        {
            Value = node.Value is { } value ? map(value) : default,
            Children = node.Children is { Count: > 0 } children ? [.. children.Select(MapCore)] : null
        };
    }
}
