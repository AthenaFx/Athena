using System;
using System.Collections.Generic;
using Consul;

namespace Athena.Consul.Discovery
{
    public class ConsulHttpCheckSettings
    {
        private readonly List<string> _tags = new List<string>();
        private string _overrideCheckName;
        private string _overrideId;
        private string _overrideCheckId;
        
        public string ApplicationName { get; private set; }

        public string CheckName => !string.IsNullOrEmpty(_overrideCheckName)
            ? _overrideCheckName
            : $"Service '{ApplicationName}' ttl check";

        public string Id => !string.IsNullOrEmpty(_overrideId)
            ? _overrideId
            : $"{Environment.MachineName.ToLower()}-{ApplicationName.ToLower()}";

        public string CheckId => !string.IsNullOrEmpty(_overrideCheckId)
            ? _overrideCheckId
            : $"service:{Id}:ttl";
        
        public string Address { get; private set; } = "";
        public int Port { get; private set; } = 0;
        public IReadOnlyCollection<string> Tags => _tags;
        public HealthStatus InitialStatus { get; private set; } = HealthStatus.Passing;
        public ConsulClient CLient { get; private set; } = new ConsulClient();

        public ConsulHttpCheckSettings WithApplicationName(string name)
        {
            ApplicationName = name;

            return this;
        }

        public ConsulHttpCheckSettings WithCheckName(string name)
        {
            _overrideCheckName = name;

            return this;
        }

        public ConsulHttpCheckSettings WithId(string id)
        {
            _overrideId = id;

            return this;
        }

        public ConsulHttpCheckSettings WithCheckId(string id)
        {
            _overrideCheckId = id;

            return this;
        }

        public ConsulHttpCheckSettings WithAddress(string address)
        {
            Address = address;

            return this;
        }

        public ConsulHttpCheckSettings WithPort(int port)
        {
            Port = port;

            return this;
        }

        public ConsulHttpCheckSettings WithInitialStatus(HealthStatus status)
        {
            InitialStatus = status;

            return this;
        }

        public ConsulHttpCheckSettings WithClient(ConsulClient client)
        {
            CLient = client;

            return this;
        }

        public ConsulHttpCheckSettings Tag(string tag)
        {
            _tags.Add(tag);

            return this;
        }
    }
}