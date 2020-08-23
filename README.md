# DotNet.DynamoDB.TestUtilities
Test utilities for working with DynamoDB in .Net

## DynamoDBLocalFixture
This class provides an xUnit fixture for working with DynamoDB local. The fixture will spin up a new Docker container with DynamoDB local, running on a random port, and make that instance available for tests. The fixture also provides table creation and deletion so tests can be run on a clean version of the table(s).

> Use of this fixture requires that Docker is installed on the host system.

See the [examples](./test/DynamoDB.TestUtilities.Examples) for now to use the text fixture in your tests.
