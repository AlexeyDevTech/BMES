using BMES.Contracts.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BMES.Infrastructure.Services
{
    public class TagConfigurationService : ITagConfigurationService
    {
        private readonly Dictionary<string, string> _tagNodeIds;

        public TagConfigurationService()
        {
            var filePath = "opc_tags.json";
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var tags = JsonSerializer.Deserialize<List<TagConfiguration>>(json);
                _tagNodeIds = tags.ToDictionary(t => t.Name, t => t.NodeId);
            }
            else
            {
                _tagNodeIds = new Dictionary<string, string>();
            }
        }

        public string GetNodeId(string tagName)
        {
            return _tagNodeIds.GetValueOrDefault(tagName, string.Empty);
        }

        private class TagConfiguration
        {
            public string Name { get; set; }
            public string NodeId { get; set; }
        }
    }
}
