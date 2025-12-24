using System.Collections.Generic;

namespace ExchangeRateUpdater.Domain;

public interface IExchangeRateProvider
{
    IEnumerable<ExchangeRate> GetExchangeRates(IEnumerable<Currency> currencies);
}