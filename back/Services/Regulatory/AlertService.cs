using back.Data.Repos.Interfaces;
using back.Models.DTOs;

namespace back.Services.Regulatory;

public class AlertService : IAlertService
{
    private readonly IRegulatoryAlertRepository _alertRepository;

    public AlertService(IRegulatoryAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public async Task<IEnumerable<RegulatoryAlertDto>> GetAlertsAsync(long userId, bool? isRead = null, CancellationToken ct = default)
    {
        var alerts = await _alertRepository.GetByUserIdAsync(userId, isRead, ct);

        return alerts.Select(a => new RegulatoryAlertDto(
            a.Id,
            a.DocumentId,
            a.Document.FileName,
            a.RegulatoryUpdate.Title,
            a.RegulatoryUpdate.Description,
            a.LegalReference != null
                ? $"{a.LegalReference.Title} — {a.LegalReference.ArticleOrSection}"
                : a.RegulatoryUpdate.LawIdentifier,
            a.RegulatoryUpdate.EffectiveDate,
            a.IsRead,
            a.CreatedAt,
            a.RiskDescription
        ));
    }

    public async Task<int> GetUnreadCountAsync(long userId, CancellationToken ct = default)
    {
        return await _alertRepository.GetUnreadCountAsync(userId, ct);
    }

    public async Task<bool> MarkAsReadAsync(Guid alertId, long userId, CancellationToken ct = default)
    {
        var alert = await _alertRepository.GetByIdAndUserIdAsync(alertId, userId, ct);
        if (alert == null) return false;

        alert.IsRead = true;
        alert.ReadAt = DateTime.UtcNow;
        await _alertRepository.UpdateAsync(alert, ct);
        return true;
    }

    public async Task MarkAllAsReadAsync(long userId, CancellationToken ct = default)
    {
        await _alertRepository.MarkAllReadAsync(userId, ct);
    }

    public async Task<bool> DismissAsync(Guid alertId, long userId, CancellationToken ct = default)
    {
        var alert = await _alertRepository.GetByIdAndUserIdAsync(alertId, userId, ct);
        if (alert == null) return false;

        alert.IsDismissed = true;
        await _alertRepository.UpdateAsync(alert, ct);
        return true;
    }
}
