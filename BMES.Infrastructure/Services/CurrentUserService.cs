using BMES.Contracts.Interfaces;

namespace BMES.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public string UserName => "TestUser"; // Hardcoded for now

        public bool HasPermission(string permissionName)
        {
            // For now, grant all permissions. In a real app, this would check against roles/permissions.
            return true;
        }
    }
}
