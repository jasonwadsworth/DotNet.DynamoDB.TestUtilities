using Xunit;

namespace DynamoDB.TestUtilities.Examples
{
    [CollectionDefinition("MyTestCollection")]
    public class MyTestCollection : ICollectionFixture<MyTestFixture>
    {
    }
}
