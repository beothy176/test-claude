using System.Collections.Generic;
using System.Linq;
using VToolProMerge.Models;

namespace VToolProMerge.Services;

public class AuditLogService
{
    private readonly List<AuditLogItem> _logs = new();

    public void WriteLog(AuditLogItem item) => _logs.Add(item);

    public IReadOnlyList<AuditLogItem> GetRecentLogs(int top = 50)
        => _logs.OrderByDescending(l => l.Time).Take(top).ToList();

    public IReadOnlyList<AuditLogItem> GetHistoryByProfile(string profile)
        => _logs.Where(l => l.Profile == profile).ToList();
}
