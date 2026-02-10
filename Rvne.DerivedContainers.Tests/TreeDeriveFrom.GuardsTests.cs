namespace Rvne.DerivedContainers.Tests;

public class TreeDeriveFromGuardsTests
{
    [Fact]
    public void DeriveFrom_ThrowsArgumentNullException_WhenInstanceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => Tree<Marker>.DeriveFrom(null!));
    }
}
