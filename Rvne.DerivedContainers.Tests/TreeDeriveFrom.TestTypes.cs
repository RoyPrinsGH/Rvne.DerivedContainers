namespace Rvne.DerivedContainers.Tests;

public interface IMarker
{
    string Id { get; }
}

public sealed class Marker(string id) : IMarker
{
    public string Id { get; } = id;
}

public sealed class IntermediateNode
{
    public Marker? Marker { get; set; }
}

public sealed class PropertyContainer
{
    public IntermediateNode? Child { get; set; }
}

public sealed class FieldContainer
{
    public IntermediateNode? Child;
}

public sealed class PrivateContainer
{
    private IntermediateNode? Child { get; set; }

    public void SetChild(IntermediateNode child) => Child = child;
}

public sealed class StaticContainer
{
    public static IntermediateNode? Child { get; set; }
}

public sealed class IndexerContainer
{
    public IntermediateNode this[int _] => new() { Marker = new Marker("indexer") };
}

public sealed class StructContainer
{
    public WrapperStruct Data { get; set; }
}

public struct WrapperStruct
{
    public Marker? Marker { get; set; }
}

public sealed class StringContainer
{
    public string? Value { get; set; }
}

public sealed class MixedContainer
{
    public IntermediateNode? Match { get; set; }
    public NoMatchLeaf? NoMatch { get; set; }
}

public sealed class NoMatchLeaf
{
    public int Count { get; set; }
}

public sealed class CyclicNode
{
    public CyclicNode? Next { get; set; }
    public Marker? Marker { get; set; }
}

public sealed class SharedHolderContainer
{
    public SharedHolder? Left { get; set; }
    public SharedHolder? Right { get; set; }
}

public sealed class SharedHolder
{
    public Marker? Marker { get; set; }
}

public sealed class SharedMarkerContainer
{
    public Marker? Left { get; set; }
    public Marker? Right { get; set; }
}

public sealed class VoidMethodContainer
{
    public IntermediateNode? Child { get; private set; }
    public int BuildCalls { get; private set; }

    public void BuildChild()
    {
        BuildCalls++;
        Child = new IntermediateNode { Marker = new Marker("generated") };
    }
}
