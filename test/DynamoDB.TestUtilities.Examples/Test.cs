using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using Xunit;

namespace DynamoDB.TestUtilities.Examples
{
    [Collection("MyTestCollection")]
    public class Test
    {
        private readonly MyTestFixture testFixture;

        public Test(MyTestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        [Fact]
        public async Task MyDynamoDBLocalTest()
        {
            await testFixture.CreateTablesAsync();
            await testFixture.AmazonDynamoDB.PutItemAsync(new PutItemRequest
            {
                ConditionExpression = "attribute_not_exists(pk)",
                Item = new Dictionary<string, AttributeValue>
                {
                    { "pk", new AttributeValue("partitionKey") },
                    { "sk", new AttributeValue("sortKey") },
                    { "someField", new AttributeValue("someValue") },
                },
                TableName = "my-table"
            });

            var response = await testFixture.AmazonDynamoDB.GetItemAsync(new GetItemRequest
            {
                Key = new Dictionary<string, AttributeValue>
                {
                    { "pk", new AttributeValue("partitionKey") },
                    { "sk", new AttributeValue("sortKey") },
                },
                TableName = "my-table"
            });

            response.IsItemSet.Should().BeTrue();
            response.Item["someField"].S.Should().Be("someValue");
        }
    }
}
