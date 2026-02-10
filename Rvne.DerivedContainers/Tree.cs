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
    public Tree<R> CallMethod<R>(string methodName, params object[] args)
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
}
