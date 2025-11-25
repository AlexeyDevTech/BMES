using Opc.Ua;
using Prism.Events;

namespace BMES.Contracts.Events
{
    public class TagValueChangedEvent : PubSubEvent<TagValue>
    {
    }

    public class TagValue
    {
        public string NodeId { get; }
        public DataValue Value { get; }

        public TagValue(string nodeId, DataValue value)
        {
            NodeId = nodeId;
            Value = value;
        }
    }
}
