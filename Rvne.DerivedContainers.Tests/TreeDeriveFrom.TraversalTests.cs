namespace Rvne.DerivedContainers.Tests;

public class TreeDeriveFromTraversalTests
{
    [Fact]
    public void DeriveFrom_AssignsValue_WhenRootInstanceIsT()
    {
        Marker marker = new("root");

        Tree<Marker> result = Tree<Marker>.DeriveFrom(marker);

        Assert.Same(marker, result.Value);
        Assert.Null(result.Children);
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
}
