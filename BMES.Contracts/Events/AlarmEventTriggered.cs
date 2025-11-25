using BMES.Core.Models;
using Prism.Events;

namespace BMES.Contracts.Events
{
    public class AlarmEventTriggered : PubSubEvent<AlarmEvent>
    {
    }
}
