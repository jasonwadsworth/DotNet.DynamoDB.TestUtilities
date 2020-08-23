using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Xunit;

namespace Wadsworth.DynamoDB.TestUtilities
{
    public abstract class DynamoDBLocalFixture : IDisposable
    {
        private readonly DynamoDBLocalHelper helper = new DynamoDBLocalHelper();

        private readonly IAmazonDynamoDB amazonDynamoDB;

        public DynamoDBLocalFixture()
        {
            var port = helper.StartDatabaseAsync().GetAwaiter().GetResult();

            amazonDynamoDB = new AmazonDynamoDBClient(new Amazon.Runtime.BasicAWSCredentials("key", "secret"), new AmazonDynamoDBConfig
            {
                ServiceURL = $"http://{ IPAddress.Loopback }:{ port }"
            });
        }

        public IAmazonDynamoDB AmazonDynamoDB
        {
            get { return amazonDynamoDB; }
        }

        protected abstract List<CreateTableRequest> GetCreateTableRequests();

        public async Task CreateTablesAsync()
        {
            var response = await amazonDynamoDB.ListTablesAsync();

            // first remove all the current tables so we are always starting fresh
            foreach (var table in response.TableNames)
            {
                await amazonDynamoDB.DeleteTableAsync(table);
            }

            var createTableRequests = GetCreateTableRequests();

            // now add all the tables the user needs
            foreach (var createTableRequest in createTableRequests)
            {
                await amazonDynamoDB.CreateTableAsync(createTableRequest);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    helper.StopDatabaseAsync().Wait();
                    helper.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
