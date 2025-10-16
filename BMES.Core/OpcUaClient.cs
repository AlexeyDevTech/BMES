using BMES.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace BMES.Core
{
    /// <summary>
    /// Provides a client for communicating with OPC UA servers.
    /// Supports connecting, disconnecting, reading, and writing data.
    /// </summary>
    public class OpcUaClient : IOpcUaClient, IAsyncDisposable
    {
        private readonly string _serverUrl;
        private readonly IUserIdentity _userIdentity;
        private readonly string _securityPolicyUri;
        private readonly ILogger<OpcUaClient> _logger;
        private ClientSessionChannel _channel;
        private readonly SemaphoreSlim _channelLock = new SemaphoreSlim(1, 1);

        public OpcUaClient(
            string serverUrl,
            IUserIdentity userIdentity = null,
            string securityPolicyUri = SecurityPolicyUris.Basic256Sha256,
            ILogger<OpcUaClient> logger = null)
        {
            _serverUrl = serverUrl ?? throw new ArgumentNullException(nameof(serverUrl));
            _userIdentity = userIdentity ?? new AnonymousIdentity();
            _securityPolicyUri = securityPolicyUri;
            _logger = logger;
        }
        public OpcUaClient() : this("opc.tcp://localhost:54000") {}

        public bool IsConnected => _channel != null && _channel.State == CommunicationState.Opened;

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            await _channelLock.WaitAsync(cancellationToken);
            try
            {
                if (IsConnected)
                {
                    _logger?.LogWarning("Already connected.");
                    return;
                }

                var clientDescription = new ApplicationDescription
                {
                    ApplicationName = "FactorySim.OpcUaClient",
                    ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:FactorySim.OpcUaClient",
                    ApplicationType = ApplicationType.Client
                };

                _channel = new ClientSessionChannel(clientDescription, null, _userIdentity, _serverUrl, _securityPolicyUri);
                await _channel.OpenAsync(cancellationToken);
                _logger?.LogInformation("Connected to OPC UA server at {ServerUrl}.", _serverUrl);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Connection failed.");
                throw new OpcUaException("Failed to connect to OPC UA server.", ex);
            }
            finally
            {
                _channelLock.Release();
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            await _channelLock.WaitAsync(cancellationToken);
            try
            {
                if (_channel != null && _channel.State != CommunicationState.Closed)
                {
                    await _channel.CloseAsync(cancellationToken);
                    _logger?.LogInformation("Disconnected from OPC UA server.");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Disconnection failed.");
            }
            finally
            {
                _channel = null;
                _channelLock.Release();
            }
        }

        public async Task<T> ReadAsync<T>(string nodeId, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);

            await _channelLock.WaitAsync(cancellationToken);
            try
            {
                var request = new ReadRequest
                {
                    NodesToRead = new[]
                    {
                        new ReadValueId { NodeId = NodeId.Parse(nodeId), AttributeId = AttributeIds.Value }
                    }
                };

                var response = await _channel.ReadAsync(request, cancellationToken);
                var result = response.Results[0];

                if (StatusCode.IsBad(result.StatusCode))
                {
                    throw new OpcUaException($"Read failed with status: {result.StatusCode}.");
                }

                return (T)Convert.ChangeType(result.Value, typeof(T));
            }
            finally
            {
                _channelLock.Release();
            }
        }

        public async Task WriteAsync<T>(string nodeId, T value, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);

            await _channelLock.WaitAsync(cancellationToken);
            try
            {
                var request = new WriteRequest
                {
                    NodesToWrite = new[]
                    {
                        new WriteValue { NodeId = NodeId.Parse(nodeId), AttributeId = AttributeIds.Value, Value = new DataValue(value) }
                    }
                };

                var response = await _channel.WriteAsync(request, cancellationToken);
                var result = response.Results[0];

                if (result != StatusCodes.Good)
                {
                    throw new OpcUaException($"Write failed with status: {result}.");
                }
            }
            finally
            {
                _channelLock.Release();
            }
        }

        private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
        {
            if (!IsConnected)
            {
                await ConnectAsync(cancellationToken);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
            _channelLock.Dispose();
        }
    }

    public class OpcUaException : Exception
    {
        public OpcUaException(string message, Exception innerException = null) : base(message, innerException) { }
    }
}
