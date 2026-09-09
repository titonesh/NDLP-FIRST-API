using System;
using System.Threading.Tasks;
using NDLP_FIRST_API.Models;

namespace NDLP_FIRST_API.Services
{
    public interface IAuditService
    {
        Task LogAuditAsync(string action, string entityName, string entityId, string customerId, string userId, object oldValues, object newValues, string correlationId = null, string ipAddress = null, bool success = true);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _db;

        public AuditService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogAuditAsync(string action, string entityName, string entityId, string customerId, string userId, object oldValues, object newValues, string correlationId = null, string ipAddress = null, bool success = true)
        {
            try
            {
                var audit = new AuditLog
                {
                    AuditId = Guid.NewGuid().ToString(),
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    CustomerId = customerId,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow,
                    OldValues = oldValues != null ? Newtonsoft.Json.JsonConvert.SerializeObject(oldValues) : null,
                    NewValues = newValues != null ? Newtonsoft.Json.JsonConvert.SerializeObject(newValues) : null,
                    CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
                    IpAddress = ipAddress,
                    Status = success ? "SUCCESS" : "FAILED"
                };

                _db.AuditLogs.Add(audit);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Reliability rule: Audit logging failures must not silently crash business flow, but should be traced/handled safely.
                System.Diagnostics.Trace.TraceError($"Audit logging error: {ex.Message}");
            }
        }
    }
}
