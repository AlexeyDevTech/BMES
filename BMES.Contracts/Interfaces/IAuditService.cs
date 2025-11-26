namespace BMES.Contracts.Interfaces
{
    public interface IAuditService
    {
        void Log(string userName, string actionDescription);
    }
}