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
    /// Invokes <paramref name="methodName" /> on each node value in the tree and returns a tree containing the projected results.
    /// </summary>
    /// <param name="methodName">The instance method name to invoke on each <typeparamref name="T" /> value.</param>
    /// <param name="args">Arguments passed to each method invocation.</param>
    /// <returns>A tree with the same structure whose values are the invocation results cast to <typeparamref name="R" />.</returns>
    protected Tree<R> CallNativeMethod<R>(string methodName, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(methodName);

        var method = typeof(T).GetMethod(methodName)
            ?? throw new MissingMethodException(typeof(T).FullName, methodName);

        if (method.ReturnType != typeof(R))
        {
            throw new InvalidOperationException(
                $"Method '{typeof(T).FullName}.{methodName}' returns '{method.ReturnType.FullName}', not '{typeof(R).FullName}'.");
        }

        return CallMethodCore(this, method, args);

        static Tree<R> CallMethodCore(Tree<T> node, MethodInfo method, object[] args)
        {
            var result = new Tree<R>();

            if (node.Value is { } tInstance)
            {
                result.Value = method.Invoke(tInstance, args) is R castValue
                    ? castValue
                    : default;
            }

            if (node.Children is { Count: > 0 } children)
            {
                result.Children = [.. children.Select(child => CallMethodCore(child, method, args))];
            }

            return result;
        }
    }

    /// <summary>
    /// Projects each node value in the tree using <paramref name="map" /> and returns a tree with the same structure.
    /// </summary>
    /// <param name="map">Projection function applied to each <typeparamref name="T" /> value.</param>
    /// <returns>A tree containing projected values of type <typeparamref name="R" />.</returns>
    protected Tree<R> Map<R>(Func<T, R> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return MapCore(this, map);

        static Tree<R> MapCore(Tree<T> node, Func<T, R> map)
        {
            var result = new Tree<R>();

            if (node.Value is { } value)
            {
                result.Value = map(value);
            }

            if (node.Children is { Count: > 0 } children)
            {
                result.Children = [.. children.Select(child => MapCore(child, map))];
            }

            return result;
        }
    }
}
