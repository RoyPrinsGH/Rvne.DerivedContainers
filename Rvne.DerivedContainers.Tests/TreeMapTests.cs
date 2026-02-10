namespace Rvne.DerivedContainers.Tests;

public class TreeMapTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenMapIsNull()
    {
        Tree<string> tree = new();

        Assert.Throws<ArgumentNullException>(() => tree.Map<int>(null!));
    }

    [Fact]
    public void Map_ProjectsValues_AndPreservesStructure()
    {
        Tree<string> tree = new()
        {
            Value = "root",
            Children =
            [
                new Tree<string> { Value = "alpha" },
                new Tree<string>
                {
                    Value = null,
                    Children =
                    [
                        new Tree<string> { Value = "beta" }
                    ]
                }
            ]
        };

        Tree<int> result = tree.Map(value => value.Length);

        Assert.Equal(4, result.Value);
        Assert.NotNull(result.Children);
        Assert.Equal(2, result.Children.Count);
        Assert.Equal(5, result.Children[0].Value);

        Tree<int> secondChild = result.Children[1];
        Assert.Equal(default, secondChild.Value);
        Assert.NotNull(secondChild.Children);
        Assert.Single(secondChild.Children);
        Assert.Equal(4, secondChild.Children[0].Value);
    }

    [Fact]
    public void Map_ReturnsNullChildren_WhenSourceChildrenAreNullOrEmpty()
    {
        Tree<string> noChildren = new() { Value = "root" };
        Tree<string> emptyChildren = new() { Value = "root", Children = [] };

        Tree<int> mappedNoChildren = noChildren.Map(value => value.Length);
        Tree<int> mappedEmptyChildren = emptyChildren.Map(value => value.Length);

        Assert.Null(mappedNoChildren.Children);
        Assert.Null(mappedEmptyChildren.Children);
    }

    [Fact]
    public void Map_DoesNotInvokeMap_ForNullValues()
    {
        Tree<string> tree = new()
        {
            Value = null,
            Children = [new Tree<string> { Value = "child" }]
        };

        var calls = 0;
        Tree<int> result = tree.Map(value =>
        {
            calls++;
            return value.Length;
        });

        Assert.Equal(1, calls);
        Assert.Equal(default, result.Value);
        Assert.NotNull(result.Children);
        Assert.Single(result.Children);
        Assert.Equal(5, result.Children[0].Value);
    }
}
