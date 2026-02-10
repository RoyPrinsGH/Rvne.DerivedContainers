using System.Reflection;

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
    public Tree<R> Map<R>(Func<T, R> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return MapRecursive(this);

        Tree<R> MapRecursive(Tree<T> node) => new()
        {
            Value = node.Value is { } value ? map(value) : default,
            Children = node.Children is { Count: > 0 } children ? [.. children.Select(MapRecursive)] : null
        };
    }

    /// <summary>
    /// Derives a tree from <paramref name="instance" /> by recursively traversing its public fields and properties.
    /// </summary>
    /// <param name="instance">Root object used to build the derived tree.</param>
    /// <returns>A tree that contains values assignable to <typeparamref name="T" />.</returns>
    public Tree<T> DeriveFrom(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        return DeriveFromRecursive(instance, new HashSet<object>(ReferenceEqualityComparer.Instance), out _);
    }

    /// <summary>
    /// Recursively derives a tree node from the provided object while tracking visited references.
    /// </summary>
    /// <param name="instance">Current object being processed.</param>
    /// <param name="visited">Set of already visited reference objects to prevent cycles.</param>
    /// <param name="hasMatch">Whether this node or any descendant contains a value assignable to <typeparamref name="T" />.</param>
    /// <returns>A derived tree node for <paramref name="instance" />.</returns>
    private static Tree<T> DeriveFromRecursive(object instance, HashSet<object> visited, out bool hasMatch)
    {
        Tree<T> node = new();
        hasMatch = false;

        if (instance is T value)
        {
            node.Value = value;
            hasMatch = true;
        }

        Type instanceType = instance.GetType();
        if (instanceType == typeof(string) || instanceType.IsValueType || !visited.Add(instance))
        {
            return node;
        }

        List<Tree<T>>? children = null;
        foreach (MemberInfo member in GetMemberInfos(instanceType))
        {
            object? childInstance = member switch
            {
                PropertyInfo property => property.GetValue(instance),
                FieldInfo field => field.GetValue(instance),
                _ => null
            };
            if (childInstance is null)
                continue;

            Tree<T> child = DeriveFromRecursive(childInstance, visited, out bool childHasMatch);
            if (!childHasMatch)
                continue;

            children ??= [];
            children.Add(child);

            hasMatch = true;
        }

        node.Children = children;
        return node;
    }

    /// <summary>
    /// Gets all public instance fields and readable non-indexer properties for <paramref name="type" />.
    /// </summary>
    /// <param name="type">Type to inspect for child members.</param>
    /// <returns>An array of members used during recursive traversal.</returns>
    private static MemberInfo[] GetMemberInfos(Type type) => [.. EnumerateMembers(type)];

    /// <summary>
    /// Enumerates public instance fields and readable non-indexer properties for <paramref name="type" />.
    /// </summary>
    /// <param name="type">Type to inspect for child members.</param>
    /// <returns>Sequence of members eligible for traversal.</returns>
    private static IEnumerable<MemberInfo> EnumerateMembers(Type type)
    {
        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (property.CanRead
                && property.GetMethod is { IsStatic: false }
                && property.GetIndexParameters().Length == 0)
            {
                yield return property;
            }
        }

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!field.IsStatic)
            {
                yield return field;
            }
        }
    }
}
