using System;
using System.Collections.Generic;
using ExchangeRateUpdater.Domain;

public class ExchangeRateProvider : IExchangeRateProvider
{
    /// <summary>
    /// Returns exchange rates for specified currencies
    /// </summary>
    public IEnumerable<ExchangeRate> GetExchangeRates(IEnumerable<Currency> currencies)
    {
        throw new NotImplementedException();
    }
}