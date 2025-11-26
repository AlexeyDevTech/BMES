namespace BMES.Contracts.Interfaces
{
    public interface ICurrentUserService
    {
        bool HasPermission(string permissionName);
        string UserName { get; }
    }
}