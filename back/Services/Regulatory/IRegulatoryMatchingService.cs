using back.Data.Entities;

namespace back.Services.Regulatory;

public interface IRegulatoryMatchingService
{
    Task<int> MatchAndCreateAlertsAsync(RegulatoryUpdate update, CancellationToken ct = default);
}
