using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Wadsworth.DynamoDB.TestUtilities
{
    internal class DynamoDBLocalHelper : IDisposable
    {
        private const string ContainerName = "dynamodb-local-";
        private const string ImageName = "amazon/dynamodb-local";
        private const string ImageTag = "latest";

        private readonly DockerClient dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();

        private string containerId;

        public async Task<int> StartDatabaseAsync()
        {
            await dockerClient.Images.CreateImageAsync(new ImagesCreateParameters() { FromImage = ImageName, Tag = ImageTag }, new AuthConfig(), new Progress<JSONMessage>());

            // Create the container
            var config = new Config()
            {
                Hostname = "localhost"
            };

            // get an open port
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();

            // Configure the ports to expose
            var hostConfig = new HostConfig()
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    { $"8000/tcp", new List<PortBinding> { new PortBinding { HostIP = IPAddress.Loopback.ToString(), HostPort = port.ToString() } } },
                }
            };

            // Create the container
            var response = await dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters(config)
            {
                Image = ImageName + ":" + ImageTag,
                Name = ContainerName + Guid.NewGuid().ToString(), // the GUID makes sure we don't have clashes if there is more than one
                Tty = false,
                HostConfig = hostConfig,
            });

            containerId = response.ID;

            await dockerClient.Containers.StartContainerAsync(containerId, new ContainerStartParameters());

            return port;
        }

        public async Task StopDatabaseAsync()
        {
            await dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters());

            await dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters());
        }

        private void Log(object sender, DataReceivedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine(args.Data);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    dockerClient?.Dispose();
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
