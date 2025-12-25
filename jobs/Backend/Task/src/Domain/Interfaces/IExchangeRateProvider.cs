using ExchangeRateUpdater.Domain.Entities;

namespace ExchangeRateUpdater.Domain;

public interface IExchangeRateProvider
{
    Task<IEnumerable<ExchangeRate>> GetExchangeRatesAsync(
        IEnumerable<Currency> currencies,
        CancellationToken cancellationToken = default);
}