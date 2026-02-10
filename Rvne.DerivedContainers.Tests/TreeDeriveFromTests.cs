namespace Rvne.DerivedContainers.Tests;

public class TreeDeriveFromTests
{
    [Fact]
    public void DeriveFrom_ThrowsArgumentNullException_WhenInstanceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => Tree<Marker>.DeriveFrom(null!));
    }

    [Fact]
    public void DeriveFrom_AssignsValue_WhenRootInstanceIsT()
    {
        Marker marker = new("root");

        Tree<Marker> result = Tree<Marker>.DeriveFrom(marker);

        Assert.Same(marker, result.Value);
        Assert.Null(result.Children);
    }

    [Fact]
    public void DeriveFrom_AssignsValue_WhenRootInstanceImplementsInterfaceT()
    {
        Marker marker = new("root");

        Tree<IMarker> result = Tree<IMarker>.DeriveFrom(marker);

        Assert.Same(marker, result.Value);
    }

    [Fact]
    public void DeriveFrom_TracesPublicPropertyRecursively()
    {
        Marker marker = new("leaf");
        PropertyContainer root = new()
        {
            Child = new IntermediateNode { Marker = marker }
        };

        Tree<Marker> result = Tree<Marker>.DeriveFrom(root);

        Tree<Marker> intermediate = Assert.Single(result.Children!);
        Assert.Null(intermediate.Value);
        Tree<Marker> markerNode = Assert.Single(intermediate.Children!);
        Assert.Same(marker, markerNode.Value);
    }

    [Fact]
    public void DeriveFrom_TracesPublicFieldRecursively()
    {
        Marker marker = new("leaf");
        FieldContainer root = new()
        {
            Child = new IntermediateNode { Marker = marker }
        };

        Tree<Marker> result = Tree<Marker>.DeriveFrom(root);

        Tree<Marker> intermediate = Assert.Single(result.Children!);
        Tree<Marker> markerNode = Assert.Single(intermediate.Children!);
        Assert.Same(marker, markerNode.Value);
    }

    [Fact]
    public void DeriveFrom_IgnoresPrivateMembers()
    {
        PrivateContainer root = new();
        root.SetChild(new IntermediateNode { Marker = new Marker("hidden") });

        Tree<Marker> result = Tree<Marker>.DeriveFrom(root);

        Assert.Null(result.Value);
        Assert.Null(result.Children);
    }

    [Fact]
    public void DeriveFrom_IgnoresStaticMembers()
    {
        StaticContainer.Child = new IntermediateNode { Marker = new Marker("static") };

        try
        {
            Tree<Marker> result = Tree<Marker>.DeriveFrom(new StaticContainer());

            Assert.Null(result.Value);
            Assert.Null(result.Children);
        }
        finally
        {
            StaticContainer.Child = null;
        }
    }

    [Fact]
    public void DeriveFrom_IgnoresIndexerProperties()
    {
        IndexerContainer root = new();

        Tree<Marker> result = Tree<Marker>.DeriveFrom(root);

        Assert.Null(result.Value);
        Assert.Null(result.Children);
    }

    [Fact]
    public void DeriveFrom_DoesNotTraverseValueTypes()
    {
        StructContainer root = new()
        {
            Data = new WrapperStruct
            {
                Marker = new Marker("value-type")
            }
        };

        Tree<Marker> result = Tree<Marker>.DeriveFrom(root);

        Assert.Null(result.Value);
        Assert.Null(result.Children);
    }

    [Fact]
    public void DeriveFrom_DoesNotTraverseStrings()
    {
        StringContainer root = new()
        {
            Value = "hello"
        };

        Tree<char> result = Tree<char>.DeriveFrom(root);

        Assert.Equal(default, result.Value);
        Assert.Null(result.Children);
    }

    [Fact]
    public void DeriveFrom_PrunesBranchesWithoutMatches()
    {
        MixedContainer root = new()
        {
            Match = new IntermediateNode { Marker = new Marker("match") },
            NoMatch = new NoMatchLeaf()
        };

        Tree<Marker> result = Tree<Marker>.DeriveFrom(root);

        Assert.NotNull(result.Children);
        Assert.Single(result.Children);
    }

    [Fact]
    public void DeriveFrom_AvoidsInfiniteRecursion_OnSelfReference()
    {
        CyclicNode root = new();
        root.Next = root;
        root.Marker = new Marker("loop-safe");

        Tree<Marker> result = Tree<Marker>.DeriveFrom(root);

        Assert.NotNull(result.Children);
        Assert.Single(result.Children);
        Assert.Equal("loop-safe", result.Children[0].Value!.Id);
    }

    [Fact]
    public void DeriveFrom_PrunesSecondVisit_ForSharedNonMatchingReference()
    {
        SharedHolder shared = new()
        {
            Marker = new Marker("one")
        };

        SharedHolderContainer root = new()
        {
            Left = shared,
            Right = shared
        };

        Tree<Marker> result = Tree<Marker>.DeriveFrom(root);

        Assert.NotNull(result.Children);
        Assert.Single(result.Children);
    }

    [Fact]
    public void DeriveFrom_KeepsSecondVisit_ForSharedMatchingReference()
    {
        Marker shared = new("two");
        SharedMarkerContainer root = new()
        {
            Left = shared,
            Right = shared
        };

        Tree<Marker> result = Tree<Marker>.DeriveFrom(root);

        Assert.NotNull(result.Children);
        Assert.Equal(2, result.Children.Count);
        Assert.All(result.Children, node => Assert.Same(shared, node.Value));
    }

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
}
