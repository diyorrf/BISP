using back.Models.DTOs;

namespace back.Services.Regulatory;

public interface IAlertService
{
    Task<IEnumerable<RegulatoryAlertDto>> GetAlertsAsync(long userId, bool? isRead = null, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(long userId, CancellationToken ct = default);
    Task<bool> MarkAsReadAsync(Guid alertId, long userId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(long userId, CancellationToken ct = default);
    Task<bool> DismissAsync(Guid alertId, long userId, CancellationToken ct = default);
}
