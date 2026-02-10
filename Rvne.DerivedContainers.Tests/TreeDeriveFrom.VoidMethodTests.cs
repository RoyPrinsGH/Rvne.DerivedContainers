namespace Rvne.DerivedContainers.Tests;

public class TreeDeriveFromVoidMethodTests
{
    [Fact]
    public void DeriveFrom_DoesNotInvokeVoidMethods_WhenDiscoveringChildren()
    {
        VoidMethodContainer root = new();

        Tree<Marker> result = Tree<Marker>.DeriveFrom(root);

        Assert.Equal(0, root.BuildCalls);
        Assert.Null(root.Child);
        Assert.Null(result.Value);
        Assert.Null(result.Children);
    }
}
