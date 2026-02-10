namespace Rvne.DerivedContainers.Tests;

public class TreeDeriveFromReferenceBehaviorTests
{
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
}
