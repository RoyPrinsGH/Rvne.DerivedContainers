namespace Rvne.DerivedContainers.Tests;

public class TreeDeriveFromInterfaceTests
{
    [Fact]
    public void DeriveFrom_AssignsValue_WhenRootInstanceImplementsInterfaceT()
    {
        Marker marker = new("root");

        Tree<IMarker> result = Tree<IMarker>.DeriveFrom(marker);

        Assert.Same(marker, result.Value);
    }

    [Fact]
    public void DeriveFrom_TracesConcreteGraph_ForTreeOfInterface()
    {
        Marker marker = new("leaf");
        PropertyContainer root = new()
        {
            Child = new IntermediateNode { Marker = marker }
        };

        Tree<IMarker> result = Tree<IMarker>.DeriveFrom(root);

        Tree<IMarker> intermediate = Assert.Single(result.Children!);
        Assert.Null(intermediate.Value);
        Tree<IMarker> markerNode = Assert.Single(intermediate.Children!);
        Assert.Same(marker, markerNode.Value);
    }

    [Fact]
    public void DeriveFrom_PrunesBranchesWithoutInterfaceMatches()
    {
        MixedContainer root = new()
        {
            Match = new IntermediateNode { Marker = new Marker("match") },
            NoMatch = new NoMatchLeaf()
        };

        Tree<IMarker> result = Tree<IMarker>.DeriveFrom(root);

        Assert.NotNull(result.Children);
        Assert.Single(result.Children);
    }

    [Fact]
    public void DeriveFrom_KeepsSecondVisit_ForSharedMatchingInterfaceReference()
    {
        Marker shared = new("shared");
        SharedMarkerContainer root = new()
        {
            Left = shared,
            Right = shared
        };

        Tree<IMarker> result = Tree<IMarker>.DeriveFrom(root);

        Assert.NotNull(result.Children);
        Assert.Equal(2, result.Children.Count);
        Assert.All(result.Children, node => Assert.Same(shared, node.Value));
    }
}
