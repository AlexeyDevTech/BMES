using Prism.Commands;

namespace BMES.Contracts.Commands
{
    public static class ApplicationCommands
    {
        public static CompositeCommand AcknowledgeAlarmCommand = new CompositeCommand();
    }
}
