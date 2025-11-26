using BMES.Contracts.Interfaces;
using Microsoft.Extensions.Logging;

namespace BMES.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService> _logger;

        public AuditService(ILogger<AuditService> logger)
        {
            _logger = logger;
        }

        public void Log(string userName, string actionDescription)
        {
            _logger.LogInformation($"Audit: User '{userName}' - {actionDescription}");
        }
    }
}